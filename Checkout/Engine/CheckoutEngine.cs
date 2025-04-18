using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Entities;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Bookstore.Checkout.Accessors;

namespace Bookstore.Checkout.Engine;

public class CheckoutEngine : ICheckoutService
{
    private readonly IPaymentValidator _paymentValidator;
    private readonly IShippingCalculator _shippingCalculator;
    private readonly IOrderAccessor _orderAccessor;
    private readonly IPaymentAccessor _paymentAccessor;
    private readonly IShippingAccessor _shippingAccessor;
    private readonly ILogger<CheckoutEngine> _logger;

    public CheckoutEngine(
        IPaymentValidator paymentValidator,
        IShippingCalculator shippingCalculator,
        IOrderAccessor orderAccessor,
        IPaymentAccessor paymentAccessor,
        IShippingAccessor shippingAccessor,
        ILogger<CheckoutEngine> logger)
    {
        _paymentValidator = paymentValidator;
        _shippingCalculator = shippingCalculator;
        _orderAccessor = orderAccessor;
        _paymentAccessor = paymentAccessor;
        _shippingAccessor = shippingAccessor;
        _logger = logger;
    }

    public async Task<CheckoutResponse> ProcessCheckoutAsync(CheckoutRequest request)
    {
        try
        {
            // 1. Validate Payment
            var paymentValidation = _paymentValidator.Validate(request.Payment);
            if (!paymentValidation.IsValid)
            {
                _logger.LogWarning("Payment validation failed: {Error}", paymentValidation.ErrorMessage);
                return CheckoutResponse.Fail(paymentValidation.ErrorMessage!);
            }

            // 2. Calculate Shipping
            var shippingCost = _shippingCalculator.Calculate(request.ShippingAddress, request.ShippingMethod);
            var shippingOption = new ShippingOption(request.ShippingMethod, shippingCost);

            // 3. Create Order
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

            // 4. Process Payment
            var paymentEntity = new PaymentEntity
            {
                CreditCardNumber = MaskCardNumber(request.Payment.CardNumber),
                ExpiryDate = request.Payment.ExpiryDate,
                Amount = CalculateTotal(request.Items, shippingCost)
            };

            // 5. Save to Database
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

            _logger.LogInformation("Checkout completed for order {OrderId}", orderEntity.OrderId);
            return CheckoutResponse.Success(orderEntity.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Checkout processing failed");
            return CheckoutResponse.Fail("An error occurred during checkout");
        }
    }

    private decimal CalculateTotal(List<CartItemRequest> items, decimal shippingCost)
    {
        // In real implementation, fetch prices from catalog service
        return items.Sum(i => i.Quantity * 10.99m) + shippingCost; // Mock price
    }

    private string MaskCardNumber(string cardNumber)
    {
        // For security - only store last 4 digits
        return $"****-****-****-{cardNumber[^4..]}";
    }
}
