namespace Bookstore.Checkout.Infrastructure.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var error = context.Exception switch {
                CheckoutException ex => new ApiError(ex.ErrorCode, ex.Message),
                _ => new ApiError("server_error", "An unexpected error occurred")
            };

            context.Result = new ObjectResult(new ApiResponse<object>(null, error))
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }
}
