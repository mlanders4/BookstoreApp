public interface IShippingCalculator
{
    ShippingOption CalculateShipping(Address address, ShippingMethod method);
}

public class ShippingCalculator : IShippingCalculator
{
    private const decimal StandardShippingRate = 4.99m;
    private const decimal ExpressShippingRate = 9.99m;
    private const decimal SameDayShippingRate = 19.99m;

    public ShippingOption CalculateShipping(Address address, ShippingMethod method)
    {
        if (address == null || !address.IsValid())
            throw new ArgumentException("Invalid shipping address");

        return method switch
        {
            ShippingMethod.Standard => new ShippingOption(method, StandardShippingRate, "3-5 business days"),
            ShippingMethod.Express => new ShippingOption(method, ExpressShippingRate, "1-2 business days"),
            ShippingMethod.SameDay => new ShippingOption(method, SameDayShippingRate, "Same day"),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };
    }
}
