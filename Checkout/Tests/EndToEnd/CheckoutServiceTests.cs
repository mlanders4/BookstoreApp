using Moq;
using Xunit;

namespace Bookstore.Checkout.Tests.Core.Services
{
    public class CheckoutServiceTests
    {
        private readonly Mock<IOrderAccessor> _mockOrderAccessor = new();
        private readonly Mock<IPaymentValidator> _mockPaymentValidator = new();
        private readonly Mock<IShippingCalculator> _mockShippingCalculator = new();
        private readonly CheckoutService _service;

        public CheckoutServiceTests()
        {
            _service = new CheckoutService(
                _mockOrderAccessor.Object,
                _mockPaymentValidator.Object,
                _mockShippingCalculator.Object,
                Mock.Of<ILogger<CheckoutService>>());
        }

        [Fact]
        public async Task ProcessCheckoutAsync_ValidRequest_CreatesOrder()
        {
            // Arrange
            var request = new CheckoutRequest(
                Items: new List<CartItemDto> { new("9788414295080", "Book", 10.99m, 2) },
                ShippingAddress: new AddressDto("123 St", "City", "ST", "12345", "US"),
                PaymentMethod: new PaymentMethodDto("Visa", "4111111111111111", "12/25", "123")
            );

            _mockShippingCalculator.Setup(x => x.CalculateShippingAsync(It.IsAny<AddressDto>(), It.IsAny<List<CartItemDto>>()))
                .ReturnsAsync(5.99m);

            _mockPaymentValidator.Setup(x => x.Validate(It.IsAny<PaymentMethodDto>()))
                .Returns(PaymentValidationResult.Success());

            _mockOrderAccessor.Setup(x => x.CreateOrderAsync(It.IsAny<Order>()))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _service.ProcessCheckoutAsync(request);

            // Assert
            Assert.NotNull(result);
            _mockOrderAccessor.Verify(x => x.CreateOrderAsync(It.IsAny<Order>()), Times.Once);
        }
    }
}
