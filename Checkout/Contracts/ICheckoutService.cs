namespace Bookstore.Checkout.Contracts;

public interface ICheckoutValidatorService
{
    Task<CheckoutValidationResult> ValidateAsync(CheckoutRequest request);
}

public record CheckoutValidationResult(
    bool IsValid,
    List<CheckoutError>? Errors = null,
    decimal? CalculatedShippingCost = null,
    string? ShippingEstimate = null
)
{
    public static CheckoutValidationResult Success(decimal shippingCost, string estimate) 
        => new(true, null, shippingCost, estimate);

    public static CheckoutValidationResult Fail(List<CheckoutError> errors) 
        => new(false, errors);
}
