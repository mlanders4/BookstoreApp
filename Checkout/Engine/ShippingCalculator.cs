using System;
using System.Net.Http;
using System.Threading.Tasks;
using Bookstore.Checkout.Models.Requests;
using Bookstore.Checkout.Models.Responses;

namespace Bookstore.Checkout.Engine
{
    public class ShippingCalculator
    {
        private readonly HttpClient _httpClient;
        private const string WarehouseAddress = "1144 T St, Lincoln, NE 68588 ";
        private const string UserAgent = "BookstoreApp";

        public ShippingCalculator()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        public async Task<ShippingOption> CalculateAsync(AddressRequest address, ShippingMethod method)
        {
            try
            {
                var destination = $"{address.Street}, {address.City}, {address.PostalCode}, {address.Country}";
                double distance = await GetDistanceMilesAsync(WarehouseAddress, destination);

                return new ShippingOption(
                    Method: method,
                    Cost: CalculateCost(distance, method),
                    DeliveryEstimate: GetDeliveryEstimate(distance, method)
                );
            }
            catch
            {
                // Fallback if API fails
                return GetFallbackOption(method);
            }
        }

        private async Task<double> GetDistanceMilesAsync(string origin, string destination)
        {
            // Step 1: Geocode addresses
            var originCoords = await GeocodeAddressAsync(origin);
            var destCoords = await GeocodeAddressAsync(destination);

            // Step 2: Get driving distance
            var response = await _httpClient.GetAsync(
                $"https://router.project-osrm.org/route/v1/driving/" +
                $"{originCoords.Longitude},{originCoords.Latitude};" +
                $"{destCoords.Longitude},{destCoords.Latitude}?overview=false");

            var content = await response.Content.ReadAsStringAsync();
            var distance = System.Text.Json.JsonSerializer.Deserialize<OsrmResponse>(content)?
                .Routes?[0].Distance / 1609.34; // Convert meters to miles

            return distance ?? 50.0; // Default fallback distance
        }

        private async Task<GeoCoordinates> GeocodeAddressAsync(string address)
        {
            var response = await _httpClient.GetAsync(
                $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json");

            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<NominatimResponse[]>(content);

            return new GeoCoordinates(
                double.Parse(result[0].Lat),
                double.Parse(result[0].Lon)
            );
        }

        private decimal CalculateCost(double distance, ShippingMethod method)
        {
            return method switch
            {
                ShippingMethod.Standard => 4.99m + (decimal)distance * 0.10m,
                ShippingMethod.Express => 9.99m + (decimal)distance * 0.15m,
                ShippingMethod.SameDay => 24.99m + (decimal)distance * 0.30m,
                _ => 9.99m
            };
        }

        private string GetDeliveryEstimate(double distance, ShippingMethod method)
        {
            return method switch
            {
                ShippingMethod.Standard => $"{(int)Math.Ceiling(distance / 100)}-{(int)Math.Ceiling(distance / 100) + 1} business days",
                ShippingMethod.Express => $"1-{(int)Math.Ceiling(distance / 200) + 1} business days",
                ShippingMethod.SameDay when distance <= 25 => "Same day delivery",
                _ => "3-5 business days"
            };
        }

        private ShippingOption GetFallbackOption(ShippingMethod method)
        {
            return method switch
            {
                ShippingMethod.Standard => new ShippingOption(method, 9.99m, "3-5 business days"),
                ShippingMethod.Express => new ShippingOption(method, 14.99m, "1-2 business days"),
                _ => new ShippingOption(method, 24.99m, "Same day (if ordered before 12PM)")
            };
        }

        // Nested helper classes
        private record GeoCoordinates(double Latitude, double Longitude);
        private record NominatimResponse(string Lat, string Lon);
        private record OsrmResponse(Route[] Routes);
        private record Route(double Distance);
    }
}
