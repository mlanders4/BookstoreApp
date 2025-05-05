using Bookstore.Checkout.Models.Requests;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Collections.Generic;
using System.Linq; // Add this important using directive

namespace Bookstore.Checkout.Tests.Models.Requests
{
    public class AddressRequestTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("123 Main St", true)]
        [InlineData("A", false)] // Test with minimum invalid length
        public void Street_Validation(string street, bool expectedIsValid)
        {
            // Arrange
            var address = new AddressRequest
            {
                Street = street,
                City = "Valid City",
                PostalCode = "12345",
                Country = "US"
            };

            var context = new ValidationContext(address);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(address, context, results, true);

            // Assert
            Assert.Equal(expectedIsValid, isValid);
            
            if (!expectedIsValid)
            {
                Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddressRequest.Street)));
            }
        }

        [Theory]
        [InlineData("US", true)]
        [InlineData("CA", true)]
        [InlineData("XX", false)]
        [InlineData(null, false)]
        public void Country_Validation(string country, bool expectedIsValid)
        {
            // Arrange
            var address = new AddressRequest
            {
                Street = "123 Valid St",
                City = "Valid City",
                PostalCode = "12345",
                Country = country
            };

            // Act
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(address, 
                new ValidationContext(address), 
                results, 
                validateAllProperties: true);

            // Assert
            Assert.Equal(expectedIsValid, isValid);
            
            if (!expectedIsValid && country != null)
            {
                Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddressRequest.Country)));
            }
        }
    }

    public static class StringExtensions
    {
        public static string Repeat(this string value, int count)
        {
            return string.Concat(Enumerable.Repeat(value, count)); // Fixed extra parenthesis
        }
    }
}
