using Bookstore.Checkout.Accessors;
using Bookstore.Checkout.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Checkout.Tests.Integration
{
    public class OrderAccessorTests : IClassFixture<DatabaseFixture>
    {
        private readonly DatabaseFixture _dbFixture;

        public OrderAccessorTests(DatabaseFixture dbFixture)
        {
            _dbFixture = dbFixture;
        }

        [Fact]
        public async Task CreateOrderAsync_ValidOrder_ReturnsOrderId()
        {
            // Arrange
            var logger = Mock.Of<ILogger<OrderAccessor>>();
            var accessor = new OrderAccessor(_dbFixture.ConnectionString, logger);
            
            var order = new OrderEntity
            {
                UserId = 1,
                CartId = 1,
                Status = "pending",
                Items = new List<OrderItemEntity>
                {
                    new() { BookId = "978-3-16-148410-0", Quantity = 1, UnitPrice = 29.99m }
                }
            };

            // Act
            var orderId = await accessor.CreateOrderAsync(order);

            // Assert
            Assert.True(orderId > 0);
            
            // Verify in database
            using var connection = new SqlConnection(_dbFixture.ConnectionString);
            await connection.OpenAsync();
            
            var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM CartItem WHERE cart_id = @orderId", 
                connection);
            cmd.Parameters.AddWithValue("@orderId", orderId);
            
            var itemCount = (int)await cmd.ExecuteScalarAsync();
            Assert.Equal(1, itemCount);
        }
    }

    public class DatabaseFixture : IDisposable
    {
        public string ConnectionString { get; }

        public DatabaseFixture()
        {
            ConnectionString = "Server=(localdb)\\mssqllocaldb;Database=BookstoreCheckoutTests;Trusted_Connection=True;";
            
            // Initialize test database
            TestDatabaseInitializer.Initialize(ConnectionString);
        }

        public void Dispose()
        {
            // Cleanup test data
            TestDatabaseInitializer.Cleanup(ConnectionString);
        }
    }
}
