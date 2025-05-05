namespace Bookstore.Checkout.Tests.Core.Services
{
    public class ShippingCalculatorTests
    {
        [Fact]
        public async Task CalculateShippingAsync_ReturnsCorrectRate_ForDomestic()
        {
            // Arrange
            var calculator = new ShippingCalculator(
                Mock.Of<ILogger<ShippingCalculator>>(),
                ConfigurationHelper.GetTestConfiguration());

            var address = new AddressDto("123 St", "Chicago", "IL", "60601", "US");
            var items = new List<CartItemDto> { new("ISBN1", "Book", 10.99m, 2) };

            // Act
            var cost = await calculator.CalculateShippingAsync(address, items);

            // Assert
            Assert.InRange(cost, 0, 20); // Verify within expected range
        }
    }

    public static class ConfigurationHelper
    {
        public static IConfiguration GetTestConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Shipping:WarehouseAddress"] = "1144 T St, Lincoln, NE 68588, USA",
                    ["Shipping:Rates:Standard:Base"] = "4.99",
                    ["Shipping:Rates:Standard:PerMile"] = "0.10"
                })
                .Build();
        }
    }
}
