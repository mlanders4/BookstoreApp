using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Data.Entities;

namespace Bookstore.Checkout.Core.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderAccessor _orderAccessor;
        private readonly IPaymentValidator _paymentValidator;
        private readonly ILogger<CheckoutService> _logger;

        public CheckoutService(
            IOrderAccessor orderAccessor,
            IPaymentValidator paymentValidator,
            ILogger<CheckoutService> logger)
        {
            _orderAccessor = orderAccessor;
            _paymentValidator = paymentValidator;
            _logger = logger;
        }

        public async Task<CheckoutResult> ProcessCheckoutAsync(CheckoutRequest request)
        {
            try
            {
                // 1. Validate payment
                var validationResult = _paymentValidator.Validate(request.Payment);
                if (!validationResult.IsValid)
                    return CheckoutResult.Failure(validationResult.Errors);

                // 2. Create order
                var order = MapToOrder(request);
                var orderId = await _orderAccessor.CreateOrderAsync(order);

                return CheckoutResult.Success(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout failed for user {UserId}", request.UserId);
                throw new CheckoutException("Checkout processing error");
            }
        }

        private Order MapToOrder(CheckoutRequest request) => new()
        {
            UserId = request.UserId,
            Status = "Pending",
            Items = request.Items.Select(i => new OrderItem
            {
                BookId = i.BookId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
    }
}
