using System.Net.Http.Json;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Checkout.Engine;

public class ShippingCalculator : IShippingCalculator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _googleMapsApiKey;
    private const decimal CostPerMile = 0.15m; // Adjust based on your business needs

    public ShippingCalculator(IHttpClientFactory httpClientFactory, IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _googleMapsApiKey = config["GoogleMaps:ApiKey"];
    }

    public async Task<ShippingOption> CalculateAsync(AddressRequest address, ShippingMethod method)
    {
        ValidateAddress(address);

        // Get distance from Google Maps API
        var distance = await GetDistanceMilesAsync(
            "YOUR_WAREHOUSE_ADDRESS", // Replace with your actual warehouse address
            $"{address.Street}, {address.City}, {address.PostalCode}, {address.Country}"
        );

        decimal baseCost = method switch
        {
            ShippingMethod.Standard => CalculateStandardCost(distance),
            ShippingMethod.Express => CalculateExpressCost(distance),
            ShippingMethod.SameDay => CalculateSameDayCost(distance, address.City),
            _ => throw new ArgumentOutOfRangeException(nameof(method))
        };

        return new ShippingOption(
            Method: method,
            Cost: Math.Round(baseCost, 2),
            DeliveryEstimate: GetDeliveryEstimate(method, distance)
        );
    }

    private async Task<double> GetDistanceMilesAsync(string origin, string destination)
    {
        var client = _httpClientFactory.CreateClient("GoogleMaps");
        
        var response = await client.GetFromJsonAsync<GoogleMapsDistanceResponse>(
            $"/maps/api/distancematrix/json?units=imperial" +
            $"&origins={Uri.EscapeDataString(origin)}" +
            $"&destinations={Uri.EscapeDataString(destination)}" +
            $"&key={_googleMapsApiKey}");

        // Default to 50 miles if API fails (for project resilience)
        return response?.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.Distance?.Value / 1609.34 ?? 50.0;
    }

    private decimal CalculateStandardCost(double distance)
    {
        // $4.99 base + $0.10 per mile
        return 4.99m + (decimal)distance * 0.10m;
    }

    private decimal CalculateExpressCost(double distance)
    {
        // $9.99 base + $0.15 per mile
        return 9.99m + (decimal)distance * 0.15m;
    }

    private decimal CalculateSameDayCost(double distance, string city)
    {
        // $24.99 base + $0.30 per mile, with city discounts
        decimal cost = 24.99m + (decimal)distance * 0.30m;
        
        var discountCities = new[] { "New York", "Los Angeles", "Chicago" };
        if (discountCities.Contains(city, StringComparer.OrdinalIgnoreCase))
        {
            cost *= 0.8m; // 20% discount for major cities
        }
        
        return cost;
    }

    private string GetDeliveryEstimate(ShippingMethod method, double distance)
    {
        int days = method switch
        {
            ShippingMethod.Standard => (int)Math.Ceiling(distance / 100) + 1,
            ShippingMethod.Express => (int)Math.Ceiling(distance / 200) + 1,
            ShippingMethod.SameDay when distance <= 50 => 0,
            ShippingMethod.SameDay => 1,
            _ => 3
        };

        return days switch
        {
            0 => "Same day delivery",
            1 => "Next day delivery",
            _ => $"Estimated {days} business days"
        };
    }

    private void ValidateAddress(AddressRequest address)
    {
        if (string.IsNullOrWhiteSpace(address.PostalCode))
            throw new ArgumentException("Postal code is required");
    }
}

// Google Maps API Response Models
public class GoogleMapsDistanceResponse
{
    public Route[] Routes { get; set; }
}

public class Route
{
    public Leg[] Legs { get; set; }
}

public class Leg
{
    public Distance Distance { get; set; }
}

public class Distance
{
    public double Value { get; set; } // Distance in meters
}
