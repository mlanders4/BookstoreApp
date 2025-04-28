using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Bookstore.Checkout.Models.Entities;

namespace Bookstore.Checkout.Engine;

public class CheckoutEngine : ICheckoutService
{
    private readonly IPaymentValidator _paymentValidator;
    private readonly IShippingCalculator _shippingCalculator;
    private readonly IOrderAccessor _orderAccessor;
    private readonly IPaymentAccessor _paymentAccessor;
    private readonly IShippingAccessor _shippingAccessor;

    public CheckoutEngine(
        IPaymentValidator paymentValidator,
        IShippingCalculator shippingCalculator,
        IOrderAccessor orderAccessor,
        IPaymentAccessor paymentAccessor,
        IShippingAccessor shippingAccessor)
    {
        _paymentValidator = paymentValidator;
        _shippingCalculator = shippingCalculator;
        _orderAccessor = orderAccessor;
        _paymentAccessor = paymentAccessor;
        _shippingAccessor = shippingAccessor;
    }

    public async Task<CheckoutResponse> ProcessCheckoutAsync(CheckoutRequest request)
    {
        // ==== 1. VALIDATION PHASE ====
        var errors = new List<CheckoutError>();

        // Payment Validation (using existing IPaymentValidator)
        var paymentValidation = _paymentValidator.Validate(request.Payment);
        if (!paymentValidation.IsValid)
        {
            errors.Add(new CheckoutError(paymentValidation.ErrorMessage!, "payment_invalid"));
        }

        // Shipping Validation (using existing IShippingCalculator)
        try
        {
            var shippingOption = await _shippingCalculator.CalculateAsync(
                request.ShippingAddress, 
                request.ShippingMethod
            );
            if (!shippingOption.IsAvailable)
            {
                errors.Add(new CheckoutError("Shipping unavailable", "shipping_error"));
            }
        }
        catch
        {
            errors.Add(new CheckoutError("Failed to calculate shipping", "shipping_calculation_failed"));
        }

        // Fail fast if validation errors exist
        if (errors.Any())
        {
            return CheckoutResponse.Failure(errors);
        }

        // ==== 2. PROCESSING PHASE ====
        try
        {
            // Order Creation (your existing code)
            var orderEntity = new OrderEntity
            {
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                Status = "Pending",
                Items = request.Items.Select(i => new OrderItemEntity
                {
                    BookId = i.BookId,
                    Quantity = i.Quantity
                }).ToList()
            };

            // Payment Processing
            var paymentEntity = new PaymentEntity
            {
                CreditCardNumber = MaskCardNumber(request.Payment.CardNumber),
                ExpiryDate = request.Payment.ExpiryDate,
                Amount = CalculateTotal(request.Items, shippingOption.Cost)
            };

            // Database Persistence
            await _orderAccessor.CreateOrderAsync(orderEntity);
            await _paymentAccessor.CreatePaymentAsync(paymentEntity);
            await _shippingAccessor.CreateShippingAsync(new ShippingEntity
            {
                OrderId = orderEntity.OrderId,
                Street = request.ShippingAddress.Street,
                City = request.ShippingAddress.City,
                PostalCode = request.ShippingAddress.PostalCode,
                Country = request.ShippingAddress.Country
            });

            return CheckoutResponse.Success(orderEntity.OrderId);
        }
        catch (Exception ex)
        {
            return CheckoutResponse.Fail("Checkout processing failed");
        }
    }

    private decimal CalculateTotal(List<CartItemRequest> items, decimal shippingCost)
    {
        return items.Sum(i => i.Quantity * 10.99m) + shippingCost; // Mock price
    }

    private string MaskCardNumber(string cardNumber)
    {
        return $"****-****-****-{cardNumber[^4..]}";
    }
}
