namespace Bookstore.Checkout.Models.Responses
{
    public record ShippingOptionResponse(
        string ShippingMethod,
        decimal Cost,
        string DeliveryEstimate,
        bool IsAvailable = true,
        string Carrier = "Standard",
        string[] AdditionalServices = null
    )
    {
        // Factory method for available options
        public static ShippingOptionResponse CreateAvailable(
            string method,
            decimal cost,
            string estimate,
            string carrier = "Standard",
            string[] additionalServices = null)
        {
            return new ShippingOptionResponse(
                ShippingMethod: method,
                Cost: cost,
                DeliveryEstimate: estimate,
                IsAvailable: true,
                Carrier: carrier,
                AdditionalServices: additionalServices
            );
        }

        // Factory method for unavailable options
        public static ShippingOptionResponse CreateUnavailable(
            string method,
            string reason)
        {
            return new ShippingOptionResponse(
                ShippingMethod: method,
                Cost: 0m,
                DeliveryEstimate: $"Not available ({reason})",
                IsAvailable: false,
                Carrier: "None"
            );
        }

        // Format for UI display
        public string DisplayText => 
            $"{ShippingMethod} - {Cost:C} ({DeliveryEstimate})";

        // Helper for checking express options
        public bool IsExpress => 
            ShippingMethod.Contains("Express") || 
            ShippingMethod.Contains("Overnight");
    }
}
