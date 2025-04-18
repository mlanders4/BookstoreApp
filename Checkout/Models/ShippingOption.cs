namespace Bookstore.Checkout.Models;

public record ShippingOption(
    ShippingMethod Method,          // Standard/Express/SameDay
    decimal Cost,                   // Calculated shipping fee
    string DeliveryEstimate         // e.g., "3-5 business days"
);

public enum ShippingMethod
{
    Standard,
    Express,
    SameDay
}
