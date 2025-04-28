using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Engine;
using Bookstore.Checkout.Models.Requests;
using Xunit;

namespace Bookstore.Checkout.Tests.Unit
{
    public class PaymentValidatorTests
    {
        private readonly PaymentValidator _validator = new(Mock.Of<ILogger<PaymentValidator>>());

        [Theory]
        [InlineData("4111111111111111", true)]  // Visa
        [InlineData("5555555555554444", true)]  // Mastercard
        [InlineData("378282246310005", true)]   // Amex
        [InlineData("1234567812345670", false)] // Invalid Luhn
        public void ValidateCardNumber_ReturnsCorrectResult(string cardNumber, bool expectedValid)
        {
            // Arrange
            var payment = new PaymentInfoRequest(
                cardNumber,
                "TEST USER",
                "12/30",
                "123");

            // Act
            var result = _validator.Validate(payment);

            // Assert
            Assert.Equal(expectedValid, result.IsValid);
        }

        [Fact]
        public void Validate_ExpiredCard_ReturnsError()
        {
            var payment = new PaymentInfoRequest(
                "4111111111111111",
                "TEST USER",
                "01/20", // Expired
                "123");

            var result = _validator.Validate(payment);

            Assert.False(result.IsValid);
            Assert.Equal(ErrorCodes.PaymentExpired, result.ErrorCode);
        }

        [Theory]
        [InlineData("123", "4111111111111111", false)] // Visa needs 3
        [InlineData("1234", "378282246310005", true)]  // Amex needs 4
        public void ValidateCvv_LengthSpecificToCardType(string cvv, string cardNumber, bool expectedValid)
        {
            var payment = new PaymentInfoRequest(
                cardNumber,
                "TEST USER",
                "12/30",
                cvv);

            var result = _validator.Validate(payment);

            Assert.Equal(expectedValid, result.IsValid);
        }
    }
}
