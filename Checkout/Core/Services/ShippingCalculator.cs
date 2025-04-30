using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bookstore.Checkout.Contracts; // Added for ShippingMethod enum
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;
using Microsoft.Extensions.Logging; // Added for ILogger

namespace Bookstore.Checkout.Engine
{
    public class ShippingCalculator : IShippingCalculator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShippingCalculator> _logger; // Added logging
        
        // Your specific warehouse address
        private const string WarehouseAddress = "1144 T St, Lincoln, NE 68588, USA";

        public ShippingCalculator(ILogger<ShippingCalculator> logger) // Updated constructor
        {
            _logger = logger;
            _httpClient = new HttpClient();
            // Required by OpenStreetMap's usage policy
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookstoreCheckoutSystem/1.0 (your-email@example.com)"); 
        }

        public async Task<ShippingOptionsResponse> CalculateAsync(AddressRequest address, ShippingMethod method)
        {
            try
            {
                var destination = FormatDestinationAddress(address);
                double distance = await GetDistanceMilesAsync(WarehouseAddress, destination);

                return new ShippingOptionsResponse(
                    Method: method,
                    Cost: CalculateCost(distance, method),
                    DeliveryEstimate: GetDeliveryEstimate(distance, method)
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shipping calculation failed for {Address}", address.ToSingleLine()); // Improved error logging
                return GetFallbackOption(method);
            }
        }

        private string FormatDestinationAddress(AddressRequest address)
        {
            return $"{address.Street}, {address.City}, {address.PostalCode}, {address.Country}";
        }

        private async Task<double> GetDistanceMilesAsync(string origin, string destination)
        {
            try 
            {
                // Step 1: Geocode addresses
                var originCoords = await GeocodeAddressAsync(origin);
                var destCoords = await GeocodeAddressAsync(destination);

                // Step 2: Get driving distance
                var response = await _httpClient.GetAsync(
                    $"https://router.project-osrm.org/route/v1/driving/" +
                    $"{originCoords.Longitude},{originCoords.Latitude};" +
                    $"{destCoords.Longitude},{destCoords.Latitude}?overview=false");

                response.EnsureSuccessStatusCode(); // Throws if HTTP request failed

                var content = await response.Content.ReadAsStringAsync();
                var osrmResponse = System.Text.Json.JsonSerializer.Deserialize<OsrmResponse>(content);
                
                if (osrmResponse?.Routes == null || osrmResponse.Routes.Length == 0)
                {
                    throw new Exception("No route data received from OSRM");
                }

                return osrmResponse.Routes[0].Distance / 1609.34; // Convert meters to miles
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate distance from {Origin} to {Destination}", origin, destination);
                return 50.0; // Default fallback distance
            }
        }

        private async Task<GeoCoordinates> GeocodeAddressAsync(string address)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<NominatimResponse[]>(content);

                if (result == null || result.Length == 0)
                {
                    throw new Exception("No geocoding results found");
                }

                return new GeoCoordinates(
                    double.Parse(result[0].Lat),
                    double.Parse(result[0].Lon)
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Geocoding failed for address: {Address}", address);
                throw; // Re-throw to be handled by CalculateAsync
            }
        }

        private decimal CalculateCost(double distance, ShippingMethod method)
        {
            return method switch
            {
                ShippingMethod.Standard => 4.99m + (decimal)distance * 0.10m,
                ShippingMethod.Express => 9.99m + (decimal)distance * 0.15m,
                ShippingMethod.SameDay => 24.99m + (decimal)distance * 0.30m,
                _ => throw new ArgumentOutOfRangeException(nameof(method), // Fail fast on invalid method
            };
        }

        private string GetDeliveryEstimate(double distance, ShippingMethod method)
        {
            return method switch
            {
                ShippingMethod.Standard => $"{(int)Math.Ceiling(distance / 100)}-{(int)Math.Ceiling(distance / 100) + 1} business days",
                ShippingMethod.Express => $"1-{(int)Math.Ceiling(distance / 200) + 1} business days",
                ShippingMethod.SameDay when distance <= 25 => "Same day delivery",
                ShippingMethod.SameDay => "Next business day",
                _ => "3-5 business days"
            };
        }

        private ShippingOptionsResponse GetFallbackOption(ShippingMethod method)
        {
            _logger.LogWarning("Using fallback shipping option for {Method}", method);
            
            return method switch
            {
                ShippingMethod.Standard => new ShippingOptionsResponse(method, 9.99m, "3-5 business days"),
                ShippingMethod.Express => new ShippingOptionsResponse(method, 14.99m, "1-2 business days"),
                _ => new ShippingOptionsResponse(method, 24.99m, "Same day (if ordered before 12PM)")
            };
        }

        // Nested helper classes
        private record GeoCoordinates(double Latitude, double Longitude);
        private record NominatimResponse(string Lat, string Lon);
        private record OsrmResponse(Route[] Routes);
        private record Route(double Distance);
    }
}
