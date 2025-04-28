using System;
using System.Data;
using System.Data.SqlClient;
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
                // 1. Create the order record
                var orderId = await CreateOrderRecordAsync(connection, transaction, order);
                
                // 2. Create order items if they exist
                if (order.Items?.Count > 0)
                {
                    await CreateOrderItemsAsync(connection, transaction, orderId, order.Items);
                }

                transaction.Commit();
                _logger.LogInformation("Successfully created order {OrderId} with {ItemCount} items", 
                    orderId, order.Items?.Count ?? 0);
                return orderId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create order for user {UserId}", order.UserId);
                throw new OrderAccessException("Failed to create order", ex);
            }
        }

        private async Task<int> CreateOrderRecordAsync(SqlConnection connection, SqlTransaction transaction, OrderEntity order)
        {
            const string sql = @"
                INSERT INTO Orders (user_id, cart_id, checkout_id, date, status)
                OUTPUT INSERTED.order_id
                VALUES (@UserId, @CartId, @CheckoutId, @OrderDate, @Status)";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddWithValue("@UserId", order.UserId);
            command.Parameters.AddWithValue("@CartId", order.CartId);
            command.Parameters.AddWithValue("@CheckoutId", order.CheckoutId);
            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
            command.Parameters.AddWithValue("@Status", order.Status);

            return (int)await command.ExecuteScalarAsync();
        }

        private async Task CreateOrderItemsAsync(SqlConnection connection, SqlTransaction transaction, 
            int orderId, ICollection<OrderEntity.OrderItemEntity> items)
        {
            const string sql = @"
                INSERT INTO CartItem (cart_id, isbn, quantity, unit_price)
                VALUES (@CartId, @Isbn, @Quantity, @UnitPrice)";

            foreach (var item in items)
            {
                using var command = new SqlCommand(sql, connection, transaction);
                command.Parameters.AddWithValue("@CartId", orderId);
                command.Parameters.AddWithValue("@Isbn", item.BookId);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            const string sql = @"
                UPDATE Orders 
                SET status = @Status 
                WHERE order_id = @OrderId
                AND status NOT IN ('completed', 'cancelled')";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);
            command.Parameters.AddWithValue("@Status", status);

            try
            {
                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync();
                
                if (affectedRows == 0)
                {
                    throw new OrderAccessException($"No order updated - may already be completed/cancelled");
                }
                
                _logger.LogInformation("Updated order {OrderId} status to {Status}", orderId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", orderId);
                throw new OrderAccessException("Failed to update order status", ex);
            }
        }
    }

    public class OrderAccessException : Exception
    {
        public OrderAccessException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
