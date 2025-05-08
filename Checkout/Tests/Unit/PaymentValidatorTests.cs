using Xunit;
using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Core.Services;

namespace Bookstore.Checkout.Tests.Core.Services
{
    public class PaymentValidatorTests
    {
        private readonly PaymentValidator _validator;

        public PaymentValidatorTests()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(x => x.GetSection("Payment:AcceptedCardTypes").Get<string[]>())
                .Returns(new[] { "Visa", "Mastercard" });

            _validator = new PaymentValidator(
                Mock.Of<ILogger<PaymentValidator>>(),
                mockConfig.Object);
        }

        [Theory]
        [InlineData("4111111111111111", true)]  // Valid Visa
        [InlineData("5105105105105100", true)]  // Valid Mastercard
        [InlineData("1234567890123456", false)] // Invalid
        public void Validate_ReturnsCorrectResult(string cardNumber, bool expectedIsValid)
        {
            var payment = new PaymentMethodDto("Visa", cardNumber, "12/25", "123");
            var result = _validator.Validate(payment);
            Assert.Equal(expectedIsValid, result.IsValid);
        }
    }
}
