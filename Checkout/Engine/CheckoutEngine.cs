using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;

namespace Bookstore.Checkout.Engine;

public class CheckoutValidator
{
    private readonly IPaymentValidator _paymentValidator;
    private readonly IShippingCalculator _shippingCalculator;

    public CheckoutValidator(
        IPaymentValidator paymentValidator,
        IShippingCalculator shippingCalculator)
    {
        _paymentValidator = paymentValidator;
        _shippingCalculator = shippingCalculator;
    }

    public async Task<CheckoutValidationResult> ValidateAsync(CheckoutRequest request)
    {
        var errors = new List<CheckoutError>();

        // 1. Validate Payment (using your existing IPaymentValidator)
        var paymentValidation = _paymentValidator.Validate(request.Payment);
        if (!paymentValidation.IsValid)
        {
            errors.Add(new CheckoutError(paymentValidation.ErrorMessage!, "payment_invalid"));
        }

        // 2. Validate Shipping (using your existing IShippingCalculator)
        var shippingOption = await _shippingCalculator.CalculateAsync(
            request.ShippingAddress, 
            request.ShippingMethod
        );

        if (!shippingOption.IsAvailable)
        {
            errors.Add(new CheckoutError("Shipping method unavailable", "shipping_unavailable"));
        }

        // Return results
        return errors.Any() 
            ? CheckoutValidationResult.Fail(errors) 
            : CheckoutValidationResult.Success(shippingOption.Cost, shippingOption.DeliveryEstimate);
    }
}

// Add this record to your existing Contracts/IPaymentValidator.cs file
public record CheckoutValidationResult(
    bool IsValid,
    List<CheckoutError>? Errors = null,
    decimal? CalculatedShippingCost = null,
    string? ShippingEstimate = null
)
{
    public static CheckoutValidationResult Success(decimal cost, string estimate) 
        => new(true, null, cost, estimate);
    public static CheckoutValidationResult Fail(List<CheckoutError> errors) 
        => new(false, errors);
}
