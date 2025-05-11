using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Bookstore.Checkout.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Accessors
{
    public class OrderAccessor : IOrderAccessor
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderAccessor> _logger;

        public OrderAccessor(IConfiguration config, ILogger<OrderAccessor> logger)
        {
            // Use the team's connection string name
            _connectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection not found");
            _logger = logger;
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            const string orderSql = @"
                INSERT INTO Orders (user_id, cart_id, date, status)
                OUTPUT INSERTED.order_id
                VALUES (@UserId, @CartId, @Date, @Status)";

            const string itemsSql = @"
                INSERT INTO OrderItems (order_id, book_id, quantity, unit_price)
                VALUES (@OrderId, @BookId, @Quantity, @UnitPrice)";

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var transaction = connection.BeginTransaction();

            try
            {
                // Create order
                using var orderCommand = new SqlCommand(orderSql, connection, transaction);
                orderCommand.Parameters.AddRange(new[]
                {
                    new SqlParameter("@UserId", order.UserId),
                    new SqlParameter("@CartId", order.CartId),
                    new SqlParameter("@Date", DateTime.UtcNow.Date),
                    new SqlParameter("@Status", order.Status)
                });

                var orderId = (int)await orderCommand.ExecuteScalarAsync();

                // Add items
                if (order.Items?.Count > 0)
                {
                    foreach (var item in order.Items)
                    {
                        using var itemCommand = new SqlCommand(itemsSql, connection, transaction);
                        itemCommand.Parameters.AddRange(new[]
                        {
                            new SqlParameter("@OrderId", orderId),
                            new SqlParameter("@BookId", item.BookId),
                            new SqlParameter("@Quantity", item.Quantity),
                            new SqlParameter("@UnitPrice", item.UnitPrice)
                        });
                        await itemCommand.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
                return orderId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Failed to create order");
                throw new OrderAccessException("Order creation failed", ex);
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            const string sql = @"
                UPDATE Orders 
                SET status = @Status
                WHERE order_id = @OrderId";

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
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update status for order {OrderId}", orderId);
                throw new OrderAccessException("Status update failed", ex);
            }
        }

        public async Task<Order> GetOrderWithItemsAsync(int orderId)
        {
            const string sql = @"
                SELECT order_id, user_id, cart_id, date, status FROM Orders WHERE order_id = @OrderId;
                SELECT id, order_id, book_id, quantity, unit_price FROM OrderItems WHERE order_id = @OrderId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (!reader.HasRows || !await reader.ReadAsync())
                    return null;

                var order = new Order
                {
                    Id = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    CartId = reader.GetInt32(2),
                    OrderDate = reader.GetDateTime(3),
                    Status = reader.GetString(4),
                    Items = new List<OrderItem>()
                };

                if (reader.NextResult())
                {
                    while (await reader.ReadAsync())
                    {
                        order.Items.Add(new OrderItem
                        {
                            Id = reader.GetInt32(0),
                            OrderId = reader.GetInt32(1),
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

        public async Task<bool> ValidateOrderAsync(int orderId, string expectedStatus)
        {
            const string sql = "SELECT 1 FROM Orders WHERE order_id = @OrderId AND status = @Status";
            
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@OrderId", orderId),
                new SqlParameter("@Status", expectedStatus)
            });

            try
            {
                await connection.OpenAsync();
                return await command.ExecuteScalarAsync() != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation failed for order {OrderId}", orderId);
                throw new OrderAccessException("Order validation failed", ex);
            }
        }
    }

    public class OrderAccessException : Exception
    {
        public OrderAccessException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
