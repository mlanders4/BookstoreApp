using System;
using System.Threading.Tasks;
using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IShippingAccessor _shippingAccessor;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICheckoutService checkoutService,
            IShippingAccessor shippingAccessor,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService ?? throw new ArgumentNullException(nameof(checkoutService));
            _shippingAccessor = shippingAccessor ?? throw new ArgumentNullException(nameof(shippingAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CheckoutResponse>>> ProcessCheckout([FromBody] CheckoutRequest request)
        {
            try
            {
                _logger.LogInformation("Initiating checkout for cart {CartId}", request.CartId);
                
                var result = await _checkoutService.ProcessCheckoutAsync(request);
                
                _logger.LogInformation("Checkout completed for order {OrderId}", result.OrderId);
                return Ok(new ApiResponse<CheckoutResponse>(result));
            }
            catch (CheckoutException ex)
            {
                _logger.LogWarning(ex, "Checkout failed: {ErrorCode}", ex.ErrorCode);
                return BadRequest(new ApiResponse<object>(
                    null, 
                    new ApiError(ex.ErrorCode, ex.Message, ex.Details)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during checkout");
                return StatusCode(500, new ApiResponse<object>(
                    null,
                    new ApiError("server_error", "An unexpected error occurred")));
            }
        }

        [HttpGet("{orderId}/shipping")]
        public async Task<ActionResult<ApiResponse<Shipping>>> GetShippingDetails(int orderId)
        {
            try
            {
                var shipping = await _shippingAccessor.GetShippingByOrderIdAsync(orderId);
                
                return shipping != null 
                    ? Ok(new ApiResponse<Shipping>(shipping))
                    : NotFound(new ApiResponse<object>(
                        null,
                        new ApiError(ErrorCodes.OrderNotFound, $"Shipping details for order {orderId} not found")));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve shipping for order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<object>(
                    null,
                    new ApiError("server_error", "Failed to retrieve shipping details")));
            }
        }

        [HttpPut("{orderId}/shipping/status")]
        public async Task<IActionResult> UpdateShippingStatus(int orderId, [FromBody] ShippingStatusUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Updating shipping status for order {OrderId} to {Status}", 
                    orderId, request.Status);

                // First validate the order exists
                var shipping = await _shippingAccessor.GetShippingByOrderIdAsync(orderId);
                if (shipping == null)
                {
                    return NotFound(new ApiResponse<object>(
                        null,
                        new ApiError(ErrorCodes.OrderNotFound, $"Order {orderId} not found")));
                }

                await _shippingAccessor.UpdateShippingStatusAsync(shipping.Id, request.Status);
                
                return NoContent();
            }
            catch (ShippingAccessException ex)
            {
                _logger.LogWarning(ex, "Failed to update shipping status for order {OrderId}", orderId);
                return BadRequest(new ApiResponse<object>(
                    null,
                    new ApiError("shipping_update_failed", ex.Message)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating shipping status for order {OrderId}", orderId);
                return StatusCode(500, new ApiResponse<object>(
                    null,
                    new ApiError("server_error", "Failed to update shipping status")));
            }
        }

        [HttpGet("shipping-estimate")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetShippingEstimate(
            [FromQuery] AddressDto address,
            [FromQuery] int itemCount)
        {
            try
            {
                if (itemCount <= 0)
                {
                    return BadRequest(new ApiResponse<object>(
                        null,
                        new ApiError("invalid_request", "Item count must be positive")));
                }

                var cost = await _checkoutService.CalculateShippingEstimateAsync(address, itemCount);
                return Ok(new ApiResponse<decimal>(cost));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shipping estimate failed for {ItemCount} items", itemCount);
                return StatusCode(500, new ApiResponse<object>(
                    null,
                    new ApiError("shipping_error", "Could not calculate shipping")));
            }
        }
    }

    public record ShippingStatusUpdateRequest(string Status);
}
