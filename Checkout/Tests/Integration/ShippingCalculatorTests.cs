using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Engine;
using Bookstore.Checkout.Models.Requests;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Bookstore.Checkout.Tests.Integration
{
    public class ShippingCalculatorTests : IDisposable
    {
        private readonly MockHttpMessageHandler _mockHttpHandler;
        private readonly HttpClient _httpClient;
        private readonly ShippingCalculator _calculator;

        public ShippingCalculatorTests()
        {
            _mockHttpHandler = new MockHttpMessageHandler();
            _httpClient = new HttpClient(_mockHttpHandler);
            
            var logger = Mock.Of<ILogger<ShippingCalculator>>();
            _calculator = new ShippingCalculator(logger)
            {
                HttpClient = _httpClient // Inject mock client
            };
        }

        [Fact]
        public async Task CalculateAsync_ValidAddress_ReturnsShippingOptions()
        {
            // Mock OSRM API response
            _mockHttpHandler.SetupResponse("https://router.project-osrm.org/...", 
                @"{""routes"":[{""distance"":8046.7}]}");

            var address = new AddressRequest(
                "123 Main St", 
                "Lincoln", 
                "68508", 
                "US");
            
            var result = await _calculator.CalculateAsync(address, ShippingMethod.Standard);

            Assert.True(result.IsAvailable);
            Assert.InRange(result.Cost, 5, 15); // Validate calculated range
        }

        [Fact]
        public async Task CalculateAsync_ApiFailure_ReturnsFallbackOption()
        {
            _mockHttpHandler.SetupFailure("https://router.project-osrm.org/...");

            var result = await _calculator.CalculateAsync(
                new AddressRequest("Bad Address", "", "", ""), 
                ShippingMethod.Express);

            Assert.True(result.IsAvailable); // Should still return fallback
            Assert.Equal(14.99m, result.Cost); // Default express cost
        }

        public void Dispose() => _httpClient.Dispose();
    }

    // Helper for mocking HTTP responses
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, HttpResponseMessage> _responses = new();

        public void SetupResponse(string url, string content)
        {
            _responses[url] = new HttpResponseMessage
            {
                Content = new StringContent(content)
            };
        }

        public void SetupFailure(string url)
        {
            _responses[url] = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError
            };
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                _responses.TryGetValue(request.RequestUri.ToString(), out var response) 
                    ? response 
                    : new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}
