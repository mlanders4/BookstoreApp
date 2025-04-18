public class CheckoutEngine
{
    private readonly IPaymentValidator _paymentValidator;
    private readonly IShippingCalculator _shippingCalculator;
    private readonly IOrderProcessor _orderProcessor;

    public CheckoutEngine(
        IPaymentValidator paymentValidator,
        IShippingCalculator shippingCalculator,
        IOrderProcessor orderProcessor)
    {
        _paymentValidator = paymentValidator;
        _shippingCalculator = shippingCalculator;
        _orderProcessor = orderProcessor;
    }

    public CheckoutResult ProcessCheckout(CheckoutRequest request)
    {
        // Validate payment card format (but don't actually charge)
        var paymentValidation = _paymentValidator.Validate(request.PaymentDetails);
        if (!paymentValidation.IsValid)
        {
            return CheckoutResult.Failure(paymentValidation.ErrorMessage);
        }

        // Calculate shipping
        var shippingOption = _shippingCalculator.CalculateShipping(
            request.ShippingAddress, 
            request.SelectedShippingMethod);

        // Create order (no actual payment processing)
        var order = _orderProcessor.CreateOrder(
            request.UserId,
            request.CartItems,
            shippingOption,
            "SIMULATED-PAYMENT");

        return CheckoutResult.Success(order);
    }
}
