using Bookstore.Checkout.Contracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Bookstore.Checkout.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly ICheckoutService _checkoutService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(
            ICheckoutService checkoutService,
            ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CheckoutResponse>>> ProcessCheckout([FromBody] CheckoutRequest request)
        {
            try
            {
                _logger.LogInformation("Starting checkout for {ItemCount} items", request.Items.Count);
                
                var result = await _checkoutService.ProcessCheckoutAsync(request);
                return Ok(new ApiResponse<CheckoutResponse>(result));
            }
            catch (CheckoutException ex)
            {
                _logger.LogWarning(ex, "Checkout failed: {ErrorCode}", ex.ErrorCode);
                return BadRequest(new ApiResponse<object>(
                    null, 
                    new ApiError(ex.ErrorCode, ex.Message)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during checkout");
                return StatusCode(500, new ApiResponse<object>(
                    null,
                    new ApiError("server_error", "An unexpected error occurred")));
            }
        }

        [HttpGet("shipping-estimate")]
        public async Task<ActionResult<ApiResponse<decimal>>> GetShippingEstimate(
            [FromQuery] AddressDto address,
            [FromQuery] int itemCount)
        {
            try
            {
                var cost = await _checkoutService.EstimateShippingAsync(address, itemCount);
                return Ok(new ApiResponse<decimal>(cost));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shipping estimate failed");
                return BadRequest(new ApiResponse<object>(
                    null,
                    new ApiError("shipping_error", "Could not calculate shipping")));
            }
        }
    }
}
