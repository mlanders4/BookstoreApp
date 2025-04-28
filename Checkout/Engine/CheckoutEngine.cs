// CheckoutValidator.cs
using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Engine;

public class CheckoutValidator : ICheckoutValidator
{
    private readonly IPaymentValidator _paymentValidator;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingCalculator _shippingCalculator;
    private readonly ILogger<CheckoutValidator> _logger;

    public CheckoutValidator(
        IPaymentValidator paymentValidator,
        IInventoryService inventoryService,
        IShippingCalculator shippingCalculator,
        ILogger<CheckoutValidator> logger)
    {
        _paymentValidator = paymentValidator;
        _inventoryService = inventoryService;
        _shippingCalculator = shippingCalculator;
        _logger = logger;
    }

    public async Task<CheckoutValidationResult> ValidateAsync(CheckoutRequest request)
    {
        var errors = new List<CheckoutError>();

        // 1. Validate Payment
        var paymentValidation = _paymentValidator.Validate(request.Payment);
        if (!paymentValidation.IsValid)
        {
            _logger.LogWarning("Payment validation failed: {Error}", paymentValidation.ErrorMessage);
            errors.Add(new CheckoutError(paymentValidation.ErrorMessage!, "payment_invalid"));
        }

        // 2. Validate Inventory
        if (!await _inventoryService.ValidateItemsAsync(request.Items))
        {
            var error = "One or more items are out of stock";
            _logger.LogWarning(error);
            errors.Add(new CheckoutError(error, "inventory_unavailable"));
        }

        // 3. Validate Shipping
        try
        {
            var shippingOption = await _shippingCalculator.CalculateAsync(
                request.ShippingAddress, 
                request.ShippingMethod
            );

            if (!shippingOption.IsAvailable)
            {
                errors.Add(new CheckoutError(
                    "Shipping method unavailable", 
                    "shipping_unavailable"
                ));
            }

            return errors.Any() 
                ? CheckoutValidationResult.Fail(errors) 
                : CheckoutValidationResult.Success(shippingOption.Cost, shippingOption.DeliveryEstimate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Shipping validation failed");
            errors.Add(new CheckoutError(
                "Failed to calculate shipping", 
                "shipping_calculation_error"
            ));
            return CheckoutValidationResult.Fail(errors);
        }
    }
}
