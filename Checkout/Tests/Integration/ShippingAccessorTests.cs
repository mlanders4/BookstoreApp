using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bookstore.Checkout.Tests.Accessors
{
    public class ShippingAccessorTests : IDisposable
    {
        private readonly SqlConnection _connection;
        private readonly ShippingAccessor _accessor;

        public ShippingAccessorTests()
        {
            _connection = new SqlConnection("CheckoutTestsDB");
            _connection.Open();
            
            var logger = Mock.Of<ILogger<ShippingAccessor>>();
            _accessor = new ShippingAccessor("CheckoutTestDB", logger);
        }

        [Fact]
        public async Task CreateShippingAsync_ValidData_ReturnsShipId()
        {
            // Arrange
            var shipping = new Shipping { 
                OrderId = 1, 
                StreetAddress = "123 Test St", 
                City = "Testville", 
                PostalCode = "12345", 
                Country = "US" 
            };

            // Act
            var shipId = await _accessor.CreateShippingAsync(shipping);

            // Assert
            Assert.True(shipId > 0);
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
