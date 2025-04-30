using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Checkout.Core.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IOrderAccessor _orderAccessor;
        private readonly IPaymentValidator _paymentValidator;
        private readonly IShippingCalculator _shippingCalculator;
        private readonly ILogger<CheckoutService> _logger;
        private readonly IConfiguration _config;

        public CheckoutService(
            IOrderAccessor orderAccessor,
            IPaymentValidator paymentValidator,
            IShippingCalculator shippingCalculator,
            ILogger<CheckoutService> logger,
            IConfiguration config)
        {
            _orderAccessor = orderAccessor;
            _paymentValidator = paymentValidator;
            _shippingCalculator = shippingCalculator;
            _logger = logger;
            _config = config;
        }

        public async Task<CheckoutResponse> ProcessCheckoutAsync(CheckoutRequest request)
        {
            try
            {
                // 1. Validate payment method against config
                var allowedCards = _config.GetSection("Payment:AcceptedCardTypes").Get<string[]>();
                if (!allowedCards.Contains(request.PaymentMethod.CardType))
                {
                    throw new CheckoutException(ErrorCodes.UnsupportedCard, 
                        $"We don't accept {request.PaymentMethod.CardType} cards");
                }

                // 2. Calculate shipping
                var shippingCost = await _shippingCalculator.CalculateShippingAsync(
                    request.ShippingAddress, 
                    request.Items.Sum(i => i.Quantity));

                // 3. Validate payment
                var paymentValidation = _paymentValidator.Validate(request.PaymentMethod);
                if (!paymentValidation.IsValid)
                {
                    throw new CheckoutException(ErrorCodes.PaymentInvalid, 
                        paymentValidation.ErrorMessage);
                }

                // 4. Create order
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = Guid.Parse(request.PaymentMethod.UserId), // Assuming UserId is in payment
                    Status = "Pending",
                    TotalAmount = request.Items.Sum(i => i.Price * i.Quantity) + shippingCost,
                    Items = request.Items.Select(i => new OrderItem
                    {
                        BookId = i.BookId,
                        Quantity = i.Quantity,
                        UnitPrice = i.Price
                    }).ToList()
                };

                await _orderAccessor.CreateOrderAsync(order);

                // 5. Return response
                return new CheckoutResponse(
                    order.Id,
                    order.TotalAmount,
                    DateTime.UtcNow.AddDays(3), // Example delivery date
                    $"TRACK-{Guid.NewGuid()}",
                    PaymentStatus.Authorized);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout failed for request: {@Request}", request);
                throw;
            }
        }
    }
}
