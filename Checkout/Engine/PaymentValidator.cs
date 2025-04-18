public interface IPaymentValidator
{
    PaymentValidationResult Validate(PaymentDetails paymentDetails);
}

public class PaymentValidator : IPaymentValidator
{
    public PaymentValidationResult Validate(PaymentDetails paymentDetails)
    {
        if (paymentDetails == null)
            return PaymentValidationResult.Failure("Payment details are required");

        // Simple card number validation (just check length)
        var cardNumber = paymentDetails.CardNumber?.Replace(" ", "") ?? "";
        
        if (cardNumber.Length < 13 || cardNumber.Length > 19)
            return PaymentValidationResult.Failure("Card number must be 13-19 digits");

        if (!cardNumber.All(char.IsDigit))
            return PaymentValidationResult.Failure("Card number must contain only digits");

        if (paymentDetails.ExpiryDate < DateTime.Now)
            return PaymentValidationResult.Failure("Card has expired");

        if (string.IsNullOrWhiteSpace(paymentDetails.Cvv))
            return PaymentValidationResult.Failure("CVV is required");

        if (paymentDetails.Cvv.Length != 3 && paymentDetails.Cvv.Length != 4)
            return PaymentValidationResult.Failure("CVV must be 3 or 4 digits");

        return PaymentValidationResult.Success();
    }
}

public record PaymentValidationResult(bool IsValid, string ErrorMessage = null)
{
    public static PaymentValidationResult Success() => new(true);
    public static PaymentValidationResult Failure(string error) => new(false, error);
}
