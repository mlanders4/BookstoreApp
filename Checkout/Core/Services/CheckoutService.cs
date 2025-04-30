using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Bookstore.Checkout.Models.Entities;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace Bookstore.Checkout.Engine
{
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
            using var transactionScope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                // ==== 1. VALIDATION PHASE ====
                var validationResult = await ValidateCheckoutAsync(request);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Checkout validation failed for user {UserId}. Errors: {Errors}", 
                        request.UserId, string.Join(", ", validationResult.Errors.Select(e => e.Code)));
                    return CheckoutResponse.Failure(validationResult.Errors);
                }

                // ==== 2. PROCESSING PHASE ====
                var (orderEntity, paymentEntity) = await ProcessOrderAsync(request, validationResult.CalculatedShippingCost!.Value);

                // ==== 3. PERSISTENCE PHASE ====
                await PersistCheckoutDataAsync(orderEntity, paymentEntity, request.ShippingAddress);

                transactionScope.Complete();

                _logger.LogInformation("Checkout completed successfully for order {OrderId}", orderEntity.OrderId);
                return CheckoutResponse.Success(orderEntity.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout processing failed for user {UserId}", request.UserId);
                return CheckoutResponse.Fail("An unexpected error occurred during checkout");
            }
        }

        private async Task<CheckoutValidationResult> ValidateCheckoutAsync(CheckoutRequest request)
        {
            var errors = new List<CheckoutError>();

            // Payment Validation
            var paymentValidation = _paymentValidator.Validate(request.Payment);
            if (!paymentValidation.IsValid)
            {
                errors.Add(new CheckoutError(paymentValidation.ErrorMessage!, "payment_invalid"));
            }

            // Shipping Validation
            try
            {
                var shippingOption = await _shippingCalculator.CalculateAsync(
                    request.ShippingAddress, 
                    request.ShippingMethod);

                if (!shippingOption.IsAvailable)
                {
                    errors.Add(new CheckoutError("Shipping method unavailable", "shipping_unavailable"));
                }

                return errors.Any() 
                    ? CheckoutValidationResult.Fail(errors) 
                    : CheckoutValidationResult.Success(shippingOption.Cost, shippingOption.DeliveryEstimate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shipping validation failed");
                errors.Add(new CheckoutError("Failed to calculate shipping options", "shipping_calculation_error"));
                return CheckoutValidationResult.Fail(errors);
            }
        }

        private async Task<(OrderEntity order, PaymentEntity payment)> ProcessOrderAsync(
            CheckoutRequest request, decimal shippingCost)
        {
            // Create Order
            var orderEntity = new OrderEntity
            {
                UserId = request.UserId,
                Date = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = request.Items.Select(i => new OrderItemEntity
                {
                    BookId = i.BookId,
                    Quantity = i.Quantity,
                    UnitPrice = GetCurrentBookPrice(i.BookId) // Should come from catalog service
                }).ToList()
            };

            // Create Payment
            var paymentEntity = new PaymentEntity
            {
                CreditCardNumber = PaymentHelper.MaskCardNumber(request.Payment.CardNumber),
                ExpiryDate = DateTime.ParseExact(request.Payment.ExpiryDate, "MM/yy", CultureInfo.InvariantCulture),
                Amount = CalculateTotal(request.Items, shippingCost),
                PaymentStatus = PaymentStatus.Pending
            };

            return (orderEntity, paymentEntity);
        }

        private async Task PersistCheckoutDataAsync(
            OrderEntity orderEntity, 
            PaymentEntity paymentEntity,
            AddressRequest shippingAddress)
        {
            // Save order first to get OrderId
            orderEntity.OrderId = await _orderAccessor.CreateOrderAsync(orderEntity);
            
            // Set foreign keys
            paymentEntity.OrderId = orderEntity.OrderId;
            
            // Save payment and shipping
            await _paymentAccessor.CreatePaymentAsync(paymentEntity);
            await _shippingAccessor.CreateShippingAsync(new ShippingEntity
            {
                OrderId = orderEntity.OrderId,
                StreetAddress = shippingAddress.Street,
                City = shippingAddress.City,
                PostalCode = shippingAddress.PostalCode,
                Country = shippingAddress.Country,
                ShippingCost = paymentEntity.Amount - orderEntity.Items.Sum(i => i.UnitPrice * i.Quantity)
            });
        }

        private decimal CalculateTotal(List<CartItemRequest> items, decimal shippingCost)
        {
            // In a real implementation, get prices from a catalog service
            return items.Sum(i => i.Quantity * GetCurrentBookPrice(i.BookId)) + shippingCost;
        }

        private decimal GetCurrentBookPrice(string bookId)
        {
            // TODO: Replace with actual catalog service call
            return 10.99m; // Mock price
        }
    }

    public static class OrderStatus
    {
        public const string Pending = "pending";
        public const string Processing = "processing";
        public const string Shipped = "shipped";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";
    }

    public static class PaymentStatus
    {
        public const string Pending = "Pending";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }

    public static class PaymentHelper
    {
        public static string MaskCardNumber(string cardNumber) => 
            $"****-****-****-{cardNumber[^4..]}";
    }
}
