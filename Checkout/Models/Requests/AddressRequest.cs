using System.ComponentModel.DataAnnotations;

namespace Bookstore.Checkout.Models.Requests
{
    public record AddressRequest(
        [Required(ErrorMessage = "Street address is required")]
        [StringLength(255, ErrorMessage = "Street address too long")]
        string Street,          // Maps to ShippingDetails.street

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City name too long")]
        string City,            // Maps to ShippingDetails.city

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Invalid postal code")]
        string PostalCode,      // Maps to ShippingDetails.zip

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country name too long")]
        string Country          // Maps to ShippingDetails.country
    )
    {
        // Converts to database-ready format
        public Dictionary<string, object> ToShippingDetails(int orderId)
        {
            return new Dictionary<string, object>
            {
                ["order_id"] = orderId,
                ["street"] = this.Street,
                ["city"] = this.City,
                ["zip"] = this.PostalCode,
                ["country"] = this.Country
            };
        }

        // Validation for supported shipping countries
        public bool IsSupportedCountry()
        {
            var supportedCountries = new[] { "USA", "Canada", "UK" };
            return supportedCountries.Contains(this.Country, StringComparer.OrdinalIgnoreCase);
        }

        // Formats for address verification
        public string ToSingleLine()
            => $"{Street}, {City}, {PostalCode}, {Country}";
    }
}
