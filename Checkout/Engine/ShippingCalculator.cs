using System.Net.Http.Json;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Engine;

public class ShippingCalculator : IShippingCalculator
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ShippingCalculator> _logger;
    private const string WarehouseAddress = "1600 Amphitheatre Parkway, Mountain View, CA"; // Replace with your warehouse

    public ShippingCalculator(
        IHttpClientFactory httpClientFactory,
        ILogger<ShippingCalculator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ShippingOption> CalculateAsync(AddressRequest address, ShippingMethod method)
    {
        try
        {
            var destination = $"{address.Street}, {address.City}, {address.PostalCode}, {address.Country}";
            
            // Get distance in miles
            double distance = await GetDistanceMilesAsync(WarehouseAddress, destination);
            
            decimal cost = method switch
            {
                ShippingMethod.Standard => 4.99m + (decimal)distance * 0.10m,
                ShippingMethod.Express => 9.99m + (decimal)distance * 0.15m,
                ShippingMethod.SameDay => 24.99m + (decimal)distance * 0.30m,
                _ => 9.99m
            };

            return new ShippingOption(
                Method: method,
                Cost: Math.Round(cost, 2),
                DeliveryEstimate: GetDeliveryEstimate(distance, method)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate shipping");
            return GetFallbackOption(method);
        }
    }

    private async Task<double> GetDistanceMilesAsync(string origin, string destination)
    {
        var client = _httpClientFactory.CreateClient();
        
        // Step 1: Geocode addresses to coordinates
        var originCoords = await GeocodeAddressAsync(origin);
        var destCoords = await GeocodeAddressAsync(destination);

        // Step 2: Get driving distance
        var response = await client.GetFromJsonAsync<OsrmRouteResponse>(
            $"https://router.project-osrm.org/route/v1/driving/" +
            $"{originCoords.Longitude},{originCoords.Latitude};" +
            $"{destCoords.Longitude},{destCoords.Latitude}?overview=false");

        return response?.Routes?.FirstOrDefault()?.Distance / 1609.34 ?? 50.0; // Convert meters to miles
    }

    private async Task<GeoCoordinates> GeocodeAddressAsync(string address)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetFromJsonAsync<NominatimGeocodeResponse[]>(
                $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json");

            return new GeoCoordinates(
                double.Parse(response[0].Lat),
                double.Parse(response[0].Lon)
            );
        }
        catch
        {
            // Fallback coordinates (San Francisco)
            return new GeoCoordinates(37.7749, -122.4194);
        }
    }

    private string GetDeliveryEstimate(double distance, ShippingMethod method)
    {
        int days = method switch
        {
            ShippingMethod.Standard => (int)Math.Max(1, distance / 100),
            ShippingMethod.Express => (int)Math.Max(1, distance / 200),
            ShippingMethod.SameDay when distance <= 25 => 0,
            _ => (int)Math.Max(1, distance / 50)
        };

        return days switch
        {
            0 => "Same day delivery",
            1 => "1 business day",
            _ => $"{days} business days"
        };
    }

    private ShippingOption GetFallbackOption(ShippingMethod method)
    {
        // Default prices when API fails
        return method switch
        {
            ShippingMethod.Standard => new ShippingOption(method, 9.99m, "3-5 business days"),
            ShippingMethod.Express => new ShippingOption(method, 14.99m, "1-2 business days"),
            _ => new ShippingOption(method, 24.99m, "Same day delivery (if ordered before 12PM)")
        };
    }
}

// Supporting Models
public record GeoCoordinates(double Latitude, double Longitude);
public record NominatimGeocodeResponse(string Lat, string Lon);
public record OsrmRouteResponse(Route[] Routes);
public record Route(double Distance);
