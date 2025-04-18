// In Contracts/IPaymentValidator.cs
namespace Bookstore.Checkout.Contracts;

public interface IPaymentValidator
{
    PaymentValidationResult Validate(PaymentInfoRequest payment);
}

public record PaymentValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static PaymentValidationResult Success() => new(true);
    public static PaymentValidationResult Fail(string error) => new(false, error);
}
