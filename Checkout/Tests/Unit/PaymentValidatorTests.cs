namespace Bookstore.Checkout.Tests.Core.Services
{
    public class PaymentValidatorTests
    {
        private readonly PaymentValidator _validator = new(Mock.Of<ILogger<PaymentValidator>>());

        [Theory]
        [InlineData("4111111111111111", true)]  // Valid Visa
        [InlineData("5105105105105100", true)]  // Valid Mastercard
        [InlineData("1234567890123456", false)] // Invalid
        public void ValidateCardNumber_ReturnsCorrectResult(string cardNumber, bool expectedIsValid)
        {
            var payment = new PaymentMethodDto("Visa", cardNumber, "12/25", "123");
            var result = _validator.Validate(payment);
            Assert.Equal(expectedIsValid, result.IsValid);
        }
    }
}
