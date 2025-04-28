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
                // 1. Create the order
                var orderId = await CreateOrderRecordAsync(connection, transaction, order);
                
                // 2. Create order items
                foreach (var item in order.Items)
                {
                    await CreateOrderItemAsync(connection, transaction, orderId, item);
                }

                transaction.Commit();
                _logger.LogInformation("Created order {OrderId} with {ItemCount} items", orderId, order.Items.Count);
                return orderId;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "Failed to create order for user {UserId}", order.UserId);
                throw new OrderCreationException("Failed to create order", ex);
            }
        }

        private async Task<int> CreateOrderRecordAsync(SqlConnection connection, SqlTransaction transaction, OrderEntity order)
        {
            const string sql = @"
                INSERT INTO Orders (user_id, cart_id, checkout_id, date, status)
                OUTPUT INSERTED.order_id
                VALUES (@UserId, @CartId, @CheckoutId, @OrderDate, @Status)";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@UserId", order.UserId),
                new SqlParameter("@CartId", order.CartId),
                new SqlParameter("@CheckoutId", order.CheckoutId),
                new SqlParameter("@OrderDate", order.OrderDate),
                new SqlParameter("@Status", order.Status)
            });

            try
            {
                return (int)await command.ExecuteScalarAsync();
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                _logger.LogWarning("Order creation conflict for user {UserId}", order.UserId);
                throw new OrderCreationException("Order already exists", ex);
            }
        }

        private async Task CreateOrderItemAsync(SqlConnection connection, SqlTransaction transaction, int orderId, OrderItemEntity item)
        {
            const string sql = @"
                INSERT INTO CartItem (cart_id, isbn, quantity, unit_price)
                VALUES (@CartId, @Isbn, @Quantity, @UnitPrice)";

            using var command = new SqlCommand(sql, connection, transaction);
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@CartId", orderId),
                new SqlParameter("@Isbn", item.BookId),
                new SqlParameter("@Quantity", item.Quantity),
                new SqlParameter("@UnitPrice", item.UnitPrice)
            });

            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (SqlException ex) when (ex.Number == 547) // FK violation
            {
                _logger.LogWarning("Invalid book ISBN {Isbn} in order items", item.BookId);
                throw new OrderCreationException($"Invalid book reference: {item.BookId}", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            const string sql = @"
                UPDATE Orders 
                SET status = @Status 
                WHERE order_id = @OrderId
                AND status NOT IN ('completed', 'cancelled')"; // Prevent invalid transitions

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
                    _logger.LogWarning("Order status update failed for {OrderId} to {Status}", orderId, status);
                    throw new OrderUpdateException($"Invalid status transition for order {orderId}");
                }
                
                _logger.LogInformation("Updated order {OrderId} to status {Status}", orderId, status);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error updating order {OrderId}", orderId);
                throw new OrderUpdateException("Failed to update order status", ex);
            }
        }
    }

    public class OrderCreationException : Exception
    {
        public OrderCreationException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }

    public class OrderUpdateException : Exception
    {
        public OrderUpdateException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}
            }
        }
    }
}
