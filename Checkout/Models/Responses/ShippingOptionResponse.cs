using System;
using System.Collections.Generic;

namespace Bookstore.Checkout.Models.Responses
{
    public record ShippingOptionsResponse(
        string MethodName,          // Maps to ShippingDetails table
        decimal Cost,               // Will be stored in Orders table
        string DeliveryEstimate,    // Display only
        string Carrier = "USPS",    // Default carrier
        bool IsAvailable = true,
        string[] ServiceOptions = null
    )
    {
        // Factory method matching your DB schema
        public static ShippingOptionsResponse FromDatabase(
            string method,
            decimal baseCost,
            AddressRequest destination)
        {
            // Calculate actual cost based on your business rules
            decimal calculatedCost = CalculateShippingCost(method, baseCost, destination);
            
            return new ShippingOptionsResponse(
                MethodName: method,
                Cost: calculatedCost,
                DeliveryEstimate: GetEstimate(method, destination.Country),
                Carrier: GetCarrier(method),
                IsAvailable: true,
                ServiceOptions: GetServiceOptions(method)
            );
        }

        // Error response
        public static ShippingOptionsResponse CreateError(string method, string error)
        {
            return new ShippingOptionsResponse(
                MethodName: method,
                Cost: 0,
                DeliveryEstimate: $"Unavailable: {error}",
                IsAvailable: false
            );
        }

        // DB-ready format for ShippingDetails
        public Dictionary<string, object> ToShippingDetails(int orderId)
        {
            return new Dictionary<string, object>
            {
                ["order_id"] = orderId,
                ["carrier"] = this.Carrier,
                ["method"] = this.MethodName,
                ["cost"] = this.Cost
                // Add other fields as needed
            };
        }

        // Cost calculation logic
        private static decimal CalculateShippingCost(string method, decimal baseCost, AddressRequest destination)
        {
            // Implement your actual business logic here
            return method switch
            {
                "Standard" => baseCost,
                "Express" => baseCost * 1.5m,
                "Overnight" => baseCost * 2.5m,
                _ => baseCost
            };
        }

        private static string GetEstimate(string method, string country)
        {
            if (country.Equals("US", StringComparison.OrdinalIgnoreCase))
            {
                return method switch
                {
                    "Standard" => "3-5 business days",
                    "Express" => "2 business days",
                    "Overnight" => "Next business day",
                    _ => "Varies by location"
                };
            }
            return method switch
            {
                "Standard" => "7-14 business days",
                "Express" => "5-7 business days",
                _ => "Contact for estimate"
            };
        }

        private static string GetCarrier(string method)
        {
            return method switch
            {
                "Overnight" => "FedEx",
                "Express" => "UPS",
                _ => "USPS"
            };
        }

        private static string[] GetServiceOptions(string method)
        {
            return method switch
            {
                "Overnight" => new[] { "Signature Required", "Insurance" },
                "Express" => new[] { "Tracking" },
                _ => Array.Empty<string>()
            };
        }
    }
}
