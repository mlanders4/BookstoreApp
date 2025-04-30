namespace Bookstore.Checkout.Core.Exceptions
{
    /// <summary>
    /// Thrown when order validation fails (invalid status, missing items, etc.)
    /// </summary>
    public class OrderValidationException : CheckoutException
    {
        public int OrderId { get; }

        public OrderValidationException(int orderId, string message) 
            : base("order_validation_failed", $"Order {orderId}: {message}")
        {
            OrderId = orderId;
        }

        public OrderValidationException(int orderId, string message, Exception inner)
            : base("order_validation_failed", $"Order {orderId}: {message}", inner)
        {
            OrderId = orderId;
        }
    }
}
