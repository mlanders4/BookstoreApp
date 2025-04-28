namespace Bookstore.Checkout.Contracts;

public interface IShippingCalculator
{
    Task<ShippingOptionsResponse> CalculateAsync(AddressRequest address, ShippingMethod method);
}
