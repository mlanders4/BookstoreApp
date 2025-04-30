using Bookstore.Checkout.Data.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Bookstore.Checkout.Accessors
{
    public class OrderAccessor : IOrderAccessor
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderAccessor> _logger;

        public OrderAccessor(IConfiguration config, ILogger<OrderAccessor> logger)
        {
            _connectionString = config?.GetConnectionString("CheckoutDB") ?? 
                throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> CreateOrderAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            if (order.Id == Guid.Empty) order.Id = Guid.NewGuid();

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
            try
            {
                // 1. Create order record
                var orderId = await InsertOrderAsync(connection, transaction, order);
                
                // 2. Add items if present
                if (order.Items?.Count > 0)
                {
                    await BulkInsertItemsAsync(connection, transaction, orderId, order.Items);
                }

                transaction.Commit();
                _logger.LogInformation("Created order {OrderId} with {ItemCount} items", 
                    orderId, order.Items?.Count ?? 0);
                
                return orderId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create order");
                throw new OrderAccessException("Order creation failed", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, string status)
        {
            const string sql = @"
                UPDATE Orders 
                SET Status = @Status,
                    ModifiedDate = GETUTCDATE()
                WHERE Id = @OrderId";

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
                    throw new OrderAccessException($"Order {orderId} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order {OrderId}", orderId);
                throw new OrderAccessException("Status update failed", ex);
            }
        }

        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            const string sql = @"
                SELECT Id, UserId, Status, TotalAmount, CreatedDate 
                FROM Orders 
                WHERE Id = @OrderId;

                SELECT Id, OrderId, BookId, Quantity, UnitPrice 
                FROM OrderItems 
                WHERE OrderId = @OrderId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (!reader.HasRows || !await reader.ReadAsync())
                {
                    return null;
                }

                var order = new Order
                {
                    Id = reader.GetGuid(0),
                    UserId = reader.GetGuid(1),
                    Status = reader.GetString(2),
                    TotalAmount = reader.GetDecimal(3),
                    CreatedDate = reader.GetDateTime(4),
                    Items = new List<OrderItem>()
                };

                // Read items if available
                if (reader.NextResult())
                {
                    while (await reader.ReadAsync())
                    {
                        order.Items.Add(new OrderItem
                        {
                            Id = reader.GetGuid(0),
                            OrderId = reader.GetGuid(1),
                            BookId = reader.GetString(2),
                            Quantity = reader.GetInt32(3),
                            UnitPrice = reader.GetDecimal(4)
                        });
                    }
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
                throw new OrderAccessException("Order retrieval failed", ex);
            }
        }

        private async Task<Guid> InsertOrderAsync(SqlConnection connection, SqlTransaction transaction, Order order)
        {
            const string sql = @"
                INSERT INTO Orders (Id, UserId, Status, TotalAmount, CreatedDate) 
                OUTPUT INSERTED.Id
                VALUES (@Id, @UserId, @Status, @TotalAmount, @CreatedDate)";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@Id", order.Id),
                new SqlParameter("@UserId", order.UserId),
                new SqlParameter("@Status", order.Status),
                new SqlParameter("@TotalAmount", order.TotalAmount),
                new SqlParameter("@CreatedDate", DateTime.UtcNow)
            });

            return (Guid)await command.ExecuteScalarAsync();
        }

        private async Task BulkInsertItemsAsync(
            SqlConnection connection, 
            SqlTransaction transaction,
            Guid orderId,
            List<OrderItem> items)
        {
            using var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction)
            {
                DestinationTableName = "OrderItems",
                BatchSize = 1000
            };

            // Add column mappings
            bulkCopy.ColumnMappings.Add("Id", "Id");
            bulkCopy.ColumnMappings.Add("OrderId", "OrderId");
            bulkCopy.ColumnMappings.Add("BookId", "BookId");
            bulkCopy.ColumnMappings.Add("Quantity", "Quantity");
            bulkCopy.ColumnMappings.Add("UnitPrice", "UnitPrice");

            using var itemTable = CreateItemDataTable(orderId, items);
            await bulkCopy.WriteToServerAsync(itemTable);
        }

        private DataTable CreateItemDataTable(Guid orderId, List<OrderItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(Guid));
            table.Columns.Add("OrderId", typeof(Guid));
            table.Columns.Add("BookId", typeof(string));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("UnitPrice", typeof(decimal));

            foreach (var item in items)
            {
                table.Rows.Add(
                    item.Id != Guid.Empty ? item.Id : Guid.NewGuid(),
                    orderId,
                    item.BookId,
                    item.Quantity,
                    item.UnitPrice);
            }

            return table;
        }
    }

    public class OrderAccessException : Exception
    {
        public OrderAccessException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
