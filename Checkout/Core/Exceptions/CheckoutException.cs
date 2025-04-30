namespace Bookstore.Checkout.Core.Exceptions
{
    public class CheckoutException : Exception
    {
        public string ErrorCode { get; }

        public CheckoutException(string errorCode, string message = null)
            : base(message ?? "Checkout error occurred") 
        {
            ErrorCode = errorCode;
        }
    }
}
