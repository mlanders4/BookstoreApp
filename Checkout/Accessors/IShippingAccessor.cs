using System;
using System.Data;
using System.Data.SqlClient;
using Bookstore.Checkout.Models.Entities;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Accessors
{
    public interface IShippingAccessor
    {
        Task<int> CreateShippingAsync(ShippingEntity shipping);
        Task UpdateShippingStatusAsync(int shipId, string status);
        Task<ShippingEntity> GetShippingByOrderIdAsync(int orderId);
    }

    public class ShippingAccessor : IShippingAccessor
    {
        private readonly string _connectionString;
        private readonly ILogger<ShippingAccessor> _logger;

        public ShippingAccessor(string connectionString, ILogger<ShippingAccessor> logger)
        {
            // Use the team's connection string name
            _connectionString = config.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException("DefaultConnection not found");
            _logger = logger;
        }

        public async Task<int> CreateShippingAsync(ShippingEntity shipping)
        {
            const string sql = @"
                INSERT INTO ShippingDetails (order_id, street, city, zip, country)
                OUTPUT INSERTED.ship_id
                VALUES (@OrderId, @Street, @City, @PostalCode, @Country)";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@OrderId", shipping.OrderId),
                new SqlParameter("@Street", shipping.StreetAddress),
                new SqlParameter("@City", shipping.City),
                new SqlParameter("@PostalCode", shipping.PostalCode),
                new SqlParameter("@Country", shipping.Country)
            });

            try
            {
                await connection.OpenAsync();
                var shipId = (int)await command.ExecuteScalarAsync();
                _logger.LogInformation("Created shipping record {ShipId} for order {OrderId}", shipId, shipping.OrderId);
                return shipId;
            }
            catch (SqlException ex) when (ex.Number == 547) // FK violation
            {
                _logger.LogError(ex, "Invalid OrderId {OrderId} when creating shipping", shipping.OrderId);
                throw new ShippingAccessException($"Invalid OrderId: {shipping.OrderId}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create shipping for order {OrderId}", shipping.OrderId);
                throw new ShippingAccessException("Failed to create shipping record", ex);
            }
        }

        public async Task UpdateShippingStatusAsync(int shipId, string status)
        {
            const string sql = @"
                UPDATE ShippingDetails 
                SET status = @Status 
                WHERE ship_id = @ShipId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            
            command.Parameters.AddRange(new[]
            {
                new SqlParameter("@ShipId", shipId),
                new SqlParameter("@Status", status)
            });

            try
            {
                await connection.OpenAsync();
                int affectedRows = await command.ExecuteNonQueryAsync();
                
                if (affectedRows == 0)
                {
                    _logger.LogWarning("No shipping record found with ID {ShipId}", shipId);
                    throw new ShippingAccessException($"Shipping record {shipId} not found");
                }
                
                _logger.LogInformation("Updated shipping {ShipId} status to {Status}", shipId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating shipping {ShipId}", shipId);
                throw new ShippingAccessException("Failed to update shipping status", ex);
            }
        }

        public async Task<ShippingEntity> GetShippingByOrderIdAsync(int orderId)
        {
            const string sql = @"
                SELECT ship_id, order_id, street, city, zip, country 
                FROM ShippingDetails 
                WHERE order_id = @OrderId";

            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@OrderId", orderId);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    return new ShippingEntity
                    {
                        ShippingId = reader.GetInt32(0),
                        OrderId = reader.GetInt32(1),
                        StreetAddress = reader.GetString(2),
                        City = reader.GetString(3),
                        PostalCode = reader.GetString(4),
                        Country = reader.GetString(5)
                    };
                }
                
                _logger.LogWarning("No shipping found for order {OrderId}", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipping for order {OrderId}", orderId);
                throw new ShippingAccessException("Failed to retrieve shipping details", ex);
            }
        }
    }

    public class ShippingAccessException : Exception
    {
        public ShippingAccessException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
