namespace Bookstore.Checkout.Contracts;

public enum ShippingMethod { Standard, Express, SameDay }

public record CheckoutValidationResult(
    bool IsValid,
    List<CheckoutError>? Errors = null,
    decimal? CalculatedShippingCost = null,
    string? ShippingEstimate = null)
{
    public static CheckoutValidationResult Success(decimal shippingCost, string estimate) 
        => new(true, null, shippingCost, estimate);

    public static CheckoutValidationResult Fail(List<CheckoutError> errors) 
        => new(false, errors);
}

public record PaymentValidationResult(bool IsValid, string? ErrorMessage = null)
{
    public static PaymentValidationResult Success() => new(true);
    public static PaymentValidationResult Fail(string error) => new(false, error);
}

public record CheckoutError(string Message, string Code, string? Details = null);
