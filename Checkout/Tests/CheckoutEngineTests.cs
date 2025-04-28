using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Engine;
using Bookstore.Checkout.Models.Entities;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Checkout.Tests.EndToEnd
{
    public class CheckoutEngineTests
    {
        private readonly Mock<IPaymentValidator> _paymentValidator = new();
        private readonly Mock<IShippingCalculator> _shippingCalculator = new();
        private readonly Mock<IOrderAccessor> _orderAccessor = new();
        private readonly Mock<IPaymentAccessor> _paymentAccessor = new();
        private readonly Mock<IShippingAccessor> _shippingAccessor = new();
        private readonly Mock<ILogger<CheckoutEngine>> _logger = new();
        
        private readonly CheckoutEngine _engine;

        public CheckoutEngineTests()
        {
            _engine = new CheckoutEngine(
                _paymentValidator.Object,
                _shippingCalculator.Object,
                _orderAccessor.Object,
                _paymentAccessor.Object,
                _shippingAccessor.Object,
                _logger.Object);
        }

        [Fact]
        public async Task ProcessCheckoutAsync_ValidRequest_CompletesTransaction()
        {
            // Arrange
            var request = new CheckoutRequest(
                userId: 1,
                cartId: 100,
                payment: new PaymentInfoRequest("4111111111111111", "TEST", "12/30", "123"),
                shippingAddress: new AddressRequest("123 St", "City", "12345", "US"));

            _paymentValidator.Setup(x => x.Validate(It.IsAny<PaymentInfoRequest>()))
                .Returns(PaymentValidationResult.Success());

            _shippingCalculator.Setup(x => x.CalculateAsync(It.IsAny<AddressRequest>(), It.IsAny<ShippingMethod>()))
                .ReturnsAsync(new ShippingOptionsResponse(ShippingMethod.Standard, 5.99m, DateTime.Now.AddDays(3), "USPS"));

            _orderAccessor.Setup(x => x.CreateOrderAsync(It.IsAny<OrderEntity>()))
                .ReturnsAsync(123);

            // Act
            var result = await _engine.ProcessCheckoutAsync(request);

            // Assert
            Assert.True(result.IsSuccess);
            _paymentAccessor.Verify(x => x.CreatePaymentAsync(It.IsAny<PaymentEntity>()), Times.Once);
            _orderAccessor.Verify(x => x.UpdateOrderStatusAsync(123, "completed"), Times.Once);
        }

        [Fact]
        public async Task ProcessCheckoutAsync_ValidationFails_RollsBack()
        {
            var request = new CheckoutRequest(...);
            
            _paymentValidator.Setup(x => x.Validate(It.IsAny<PaymentInfoRequest>()))
                .Returns(PaymentValidationResult.Fail(ErrorCodes.PaymentInvalid, "Invalid card"));

            var result = await _engine.ProcessCheckoutAsync(request);

            Assert.False(result.IsSuccess);
            _orderAccessor.Verify(x => x.CreateOrderAsync(It.IsAny<OrderEntity>()), Times.Never);
        }
    }
}
