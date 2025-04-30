namespace Bookstore.Checkout.Core.Exceptions
{
    public class CheckoutException : Exception
    {
        public string ErrorCode { get; }
        public Dictionary<string, object> Details { get; }

        public CheckoutException(string errorCode, string message = null, 
            Dictionary<string, object> details = null)
            : base(message ?? "Checkout error occurred")
        {
            ErrorCode = errorCode;
            Details = details ?? new Dictionary<string, object>();
        }

        public CheckoutException(string errorCode, string message, Exception inner,
            Dictionary<string, object> details = null)
            : base(message, inner)
        {
            ErrorCode = errorCode;
            Details = details ?? new Dictionary<string, object>();
        }
    }
}
