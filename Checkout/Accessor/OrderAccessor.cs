using System;
using System.Data;
using System.Data.SqlClient;
using Bookstore.Checkout.Models.Entities;
using Bookstore.Checkout.Models.Requests;

namespace Bookstore.Checkout.Accessors
{
    public class OrderAccessor : IOrderAccessor
    {
        private readonly string _connectionString;

        public OrderAccessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<int> CreateOrderAsync(OrderEntity order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // 1. Create the order
                var orderId = await CreateOrderRecordAsync(connection, order);
                
                // 2. Create order items
                foreach (var item in order.Items)
                {
                    await CreateOrderItemAsync(connection, orderId, item);
                }

                return orderId;
            }
        }

        private async Task<int> CreateOrderRecordAsync(SqlConnection connection, OrderEntity order)
        {
            const string sql = @"
                INSERT INTO Orders (user_id, cart_id, checkout_id, date, status)
                OUTPUT INSERTED.order_id
                VALUES (@UserId, @CartId, @CheckoutId, @OrderDate, @Status)";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@UserId", order.UserId);
                command.Parameters.AddWithValue("@CartId", order.CartId);
                command.Parameters.AddWithValue("@CheckoutId", order.CheckoutId);
                command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                command.Parameters.AddWithValue("@Status", order.Status);

                return (int)await command.ExecuteScalarAsync();
            }
        }

        private async Task CreateOrderItemAsync(SqlConnection connection, int orderId, OrderItemEntity item)
        {
            const string sql = @"
                INSERT INTO CartItem (cart_id, isbn, quantity)
                VALUES (@CartId, @Isbn, @Quantity)";

            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@CartId", orderId); // Using orderId as cart_id
                command.Parameters.AddWithValue("@Isbn", item.BookId);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status)
        {
            const string sql = "UPDATE Orders SET status = @Status WHERE order_id = @OrderId";
            
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@OrderId", orderId);
                command.Parameters.AddWithValue("@Status", status);
                
                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
