using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Bookstore.Checkout.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Accessors
{
    public class OrderAccessor : IOrderAccessor
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderAccessor> _logger;

        public OrderAccessor(string connectionString, ILogger<OrderAccessor> logger)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> CreateOrderAsync(OrderEntity order)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();
            try
            {
                // 1. Create order record
                var orderId = await CreateOrderRecordAsync(connection, transaction, order);
                
                // 2. Bulk insert items
                if (order.Items?.Count > 0)
                {
                    await BulkInsertOrderItemsAsync(connection, transaction, orderId, order.Items);
                }

                transaction.Commit();
                
                _logger.LogInformation("Created order {OrderId} with {ItemCount} items for user {UserId}", 
                    orderId, order.Items?.Count ?? 0, order.UserId);
                
                return orderId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create order for user {UserId}. Items: {ItemCount}", 
                    order.UserId, order.Items?.Count ?? 0);
                throw new OrderAccessException("Order creation failed", ex);
            }
        }

        private async Task<int> CreateOrderRecordAsync(SqlConnection connection, SqlTransaction transaction, OrderEntity order)
        {
            const string sql = @"
                INSERT INTO Orders (
                    user_id, 
                    cart_id, 
                    date, 
                    status, 
                    total_amount
                )
                OUTPUT INSERTED.order_id
                VALUES (
                    @UserId, 
                    @CartId, 
                    @OrderDate, 
                    @Status, 
                    @TotalAmount
                )";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@UserId", order.UserId),
                new SqlParameter("@CartId", order.CartId),
                new SqlParameter("@OrderDate", order.OrderDate),
                new SqlParameter("@Status", order.Status),
                new SqlParameter("@TotalAmount", order.TotalAmount ?? (object)DBNull.Value)
            });

            return (int)await command.ExecuteScalarAsync();
        }

        private async Task BulkInsertOrderItemsAsync(
            SqlConnection connection, 
            SqlTransaction transaction,
            int orderId,
            ICollection<OrderItemEntity> items)
        {
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = "CartItem",
                BatchSize = 1000
            };

            // Column mappings
            bulkCopy.ColumnMappings.Add("OrderId", "cart_id");
            bulkCopy.ColumnMappings.Add("BookId", "isbn");
            bulkCopy.ColumnMappings.Add("Quantity", "quantity");
            bulkCopy.ColumnMappings.Add("UnitPrice", "unit_price");

            // Prepare DataTable
            var itemTable = new DataTable();
            itemTable.Columns.Add("OrderId", typeof(int));
            itemTable.Columns.Add("BookId", typeof(string));
            itemTable.Columns.Add("Quantity", typeof(int));
            itemTable.Columns.Add("UnitPrice", typeof(decimal));

            foreach (var item in items)
            {
                itemTable.Rows.Add(
                    orderId,
                    item.BookId,
                    item.Quantity,
                    item.UnitPrice
                );
            }

            await bulkCopy.WriteToServerAsync(itemTable);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            const string sql = @"
                UPDATE Orders 
                SET 
                    status = @Status,
                    date = CASE 
                        WHEN @Status = 'completed' THEN GETUTCDATE()
                        ELSE date
                    END
                WHERE order_id = @OrderId
                AND status NOT IN ('completed', 'cancelled')";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Status", status)
            });

            try
            {
                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync();
                
                if (affectedRows == 0)
                {
                    _logger.LogWarning("Status update blocked for order {OrderId}. Current status may be final.", orderId);
                    throw new OrderAccessException($"Order {orderId} cannot transition to {status}");
                }
                
                _logger.LogInformation("Updated order {OrderId} to status {Status}", orderId, status);
            }
            catch (SqlException ex) when (ex.Number == 547) // FK violation
            {
                _logger.LogError(ex, "Invalid order ID {OrderId}", orderId);
                throw new OrderAccessException($"Order {orderId} not found", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order {OrderId}", orderId);
                throw new OrderAccessException("Status update failed", ex);
            }
        }

        public async Task<OrderEntity> GetOrderAsync(int orderId)
        {
            const string sql = @"
                SELECT 
                    order_id, user_id, cart_id, checkout_id, 
                    date, status, total_amount
                FROM Orders 
                WHERE order_id = @OrderId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (!await reader.ReadAsync())
                {
                    _logger.LogWarning("Order {OrderId} not found", orderId);
                    return null;
                }

                return new OrderEntity
                {
                    OrderId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CartId = reader.GetInt32(2),
                    CheckoutId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    OrderDate = reader.GetDateTime(4),
                    Status = reader.GetString(5),
                    TotalAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
                throw new OrderAccessException("Order retrieval failed", ex);
            }
        }
    }

    public class OrderAccessException : Exception
    {
        public OrderAccessException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
