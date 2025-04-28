namespace Bookstore.Checkout.Contracts;

public interface ICheckoutService
{
    Task<CheckoutResponse> ProcessCheckoutAsync(CheckoutRequest request);
}
