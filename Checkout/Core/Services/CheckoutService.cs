using System;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Data.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
            _orderAccessor = orderAccessor ?? throw new ArgumentNullException(nameof(orderAccessor));
            _paymentValidator = paymentValidator ?? throw new ArgumentNullException(nameof(paymentValidator));
            _shippingCalculator = shippingCalculator ?? throw new ArgumentNullException(nameof(shippingCalculator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<CheckoutResponse> ProcessCheckoutAsync(CheckoutRequest request)
        {
            try
            {
                // 1. Validate payment method
                var allowedCards = _config.GetSection("Payment:AcceptedCardTypes").Get<string[]>();
                if (!allowedCards.Contains(request.PaymentMethod.CardType))
                {
                    throw new CheckoutException(ErrorCodes.UnsupportedCard, 
                        $"We don't accept {request.PaymentMethod.CardType} cards");
                }

                // 2. Validate payment details
                var paymentValidation = _paymentValidator.Validate(request.PaymentMethod);
                if (!paymentValidation.IsValid)
                {
                    throw new CheckoutException(ErrorCodes.PaymentInvalid, 
                        paymentValidation.ErrorMessage);
                }

                // 3. Calculate shipping
                var shippingCost = await _shippingCalculator.CalculateShippingAsync(
                    request.ShippingAddress, 
                    request.Items.Sum(i => i.Quantity));

                // 4. Create order
                var order = new Order
                {
                    UserId = request.UserId, // Assuming this comes from auth
                    CartId = request.CartId,
                    Status = "pending",
                    Items = request.Items.Select(i => new OrderItem
                    {
                        BookId = i.BookId,
                        Quantity = i.Quantity,
                        UnitPrice = i.Price
                    }).ToList()
                };

                var orderId = await _orderAccessor.CreateOrderAsync(order);

                // 5. Return response
                return new CheckoutResponse(
                    orderId,
                    request.Items.Sum(i => i.Price * i.Quantity) + shippingCost,
                    DateTime.UtcNow.AddDays(3),
                    $"TRACK-{orderId}",
                    PaymentStatus.Authorized);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Database error during checkout");
                throw new CheckoutException("checkout_db_error", "Database operation failed", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Shipping API failure");
                throw new CheckoutException("shipping_api_error", "Shipping service unavailable", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected checkout error");
                throw new CheckoutException("checkout_error", "Checkout process failed", ex);
            }
        }

        public async Task<decimal> CalculateShippingEstimateAsync(AddressDto address, int itemCount)
        {
            try
            {
                return await _shippingCalculator.CalculateShippingAsync(address, itemCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shipping estimate failed");
                return _config.GetValue<decimal>("Shipping:FallbackRate");
            }
        }
    }
}
