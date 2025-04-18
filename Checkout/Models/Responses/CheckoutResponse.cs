namespace Bookstore.Checkout.Models.Responses
{
    public record CheckoutResponse(
        bool IsSuccess,
        string OrderId = null,
        decimal? TotalAmount = null,
        string ShippingEstimate = null,
        string PaymentConfirmation = null,
        string ErrorMessage = null
    )
    {
        public static CheckoutResponse Success(
            string orderId,
            decimal totalAmount,
            string shippingEstimate,
            string paymentConfirmation = "Payment authorized")
        {
            return new CheckoutResponse(
                IsSuccess: true,
                OrderId: orderId,
                TotalAmount: totalAmount,
                ShippingEstimate: shippingEstimate,
                PaymentConfirmation: paymentConfirmation
            );
        }

        public static CheckoutResponse Failure(
            string errorMessage,
            string orderId = null)
        {
            return new CheckoutResponse(
                IsSuccess: false,
                OrderId: orderId,
                ErrorMessage: errorMessage
            );
        }

        // Helper method for API responses
        public object ToApiResponse()
        {
            return IsSuccess
                ? new
                {
                    Success = true,
                    OrderId,
                    TotalAmount,
                    ShippingEstimate,
                    PaymentConfirmation
                }
                : new
                {
                    Success = false,
                    OrderId,
                    ErrorMessage
                };
        }
    }
}
