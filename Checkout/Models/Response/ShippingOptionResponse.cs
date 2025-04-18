// Checkout/Models/Responses/ShippingOption.cs
public record ShippingOptionResponse(
    ShippingMethod Method,
    decimal Cost,
    string DeliveryEstimate
);

public enum ShippingMethod { Standard, Express, SameDay }
