namespace Bookstore.Checkout.Contracts
{
    public interface ICheckoutService
    {
        Task<CheckoutResponse> ProcessCheckoutAsync(CheckoutRequest request);
        Task<decimal> CalculateShippingEstimateAsync(AddressDto address, int itemCount);
    }
}
