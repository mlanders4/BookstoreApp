using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bookstore.Checkout.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookstore.Checkout.Core.Services
{
    public class ShippingCalculator : IShippingCalculator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ShippingCalculator> _logger;
        private readonly IConfiguration _config;

        public ShippingCalculator(
            IHttpClientFactory httpClientFactory,
            ILogger<ShippingCalculator> logger,
            IConfiguration config)
        {
            _httpClient = httpClientFactory?.CreateClient("ShippingAPI") ?? 
                throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "BookstoreCheckoutSystem");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<decimal> CalculateShippingAsync(AddressDto address, int itemCount)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (itemCount <= 0) throw new ArgumentOutOfRangeException(nameof(itemCount));

            try
            {
                var warehouseAddress = _config["Shipping:WarehouseAddress"] ?? 
                    throw new InvalidOperationException("Warehouse address not configured");
                
                var destination = FormatAddress(address);
                var distance = await GetDistanceMilesAsync(warehouseAddress, destination);
                var rates = _config.GetSection("Shipping:Rates").Get<ShippingRates>() ?? 
                    throw new InvalidOperationException("Shipping rates not configured");

                return CalculateCost(distance, itemCount, rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Shipping calculation failed for {Address}", 
                    address?.ToSingleLine() ?? "null address");
                
                return _config.GetValue<decimal>("Shipping:FallbackRate");
            }
        }

        private string FormatAddress(AddressDto address)
        {
            return $"{address.Street}, {address.City}, {address.ZipCode}, {address.Country}";
        }

        private async Task<double> GetDistanceMilesAsync(string origin, string destination)
        {
            try 
            {
                var originCoords = await GeocodeAddressAsync(origin);
                var destCoords = await GeocodeAddressAsync(destination);

                var response = await _httpClient.GetAsync(
                    $"https://router.project-osrm.org/route/v1/driving/" +
                    $"{originCoords.Longitude},{originCoords.Latitude};" +
                    $"{destCoords.Longitude},{destCoords.Latitude}?overview=false");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var osrmResponse = JsonSerializer.Deserialize<OsrmResponse>(content);
                
                if (osrmResponse?.Routes == null || osrmResponse.Routes.Length == 0)
                {
                    throw new Exception("No route data received from OSRM");
                }

                return osrmResponse.Routes[0].Distance / 1609.34;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate distance from {Origin} to {Destination}", 
                    origin, destination);
                throw new ShippingCalculationException("Distance calculation failed", ex);
            }
        }

        private async Task<GeoCoordinates> GeocodeAddressAsync(string address)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json");

                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<NominatimResponse[]>(stream);

                if (result == null || result.Length == 0)
                {
                    throw new Exception("No geocoding results found");
                }

                return new GeoCoordinates(
                    double.Parse(result[0].Lat),
                    double.Parse(result[0].Lon));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Geocoding failed for address: {Address}", address);
                throw;
            }
        }

        private decimal CalculateCost(double distance, int itemCount, ShippingRates rates)
        {
            var baseRate = rates.Standard.Base + (rates.Standard.PerMile * (decimal)distance);
            var total = baseRate * itemCount;
            
            var minimumCharge = _config.GetValue<decimal>("Shipping:MinimumCharge");
            return Math.Max(total, minimumCharge);
        }

        private record GeoCoordinates(double Latitude, double Longitude);
        private record NominatimResponse(string Lat, string Lon);
        private record OsrmResponse(Route[] Routes);
        private record Route(double Distance);
        private record ShippingRates(Rate Standard, Rate Express);
        private record Rate(decimal Base, decimal PerMile);
    }

    public class ShippingCalculationException : Exception
    {
        public ShippingCalculationException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}
