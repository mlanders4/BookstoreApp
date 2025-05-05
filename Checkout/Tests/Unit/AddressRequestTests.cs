using Bookstore.Checkout.Models.Requests;
using System.ComponentModel.DataAnnotations;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace Bookstore.Checkout.Tests.Models.Requests
{
    public class AddressRequestTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("123 Main St", true)]
        [InlineData("A", false)]
        public void Street_Validation(string street, bool expectedIsValid)
        {
            var address = new AddressRequest
            {
                Street = street,
                City = "Valid City",
                PostalCode = "12345",
                Country = "US"
            };

            var context = new ValidationContext(address);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(address, context, results, true);

            Assert.Equal(expectedIsValid, isValid);
            
            if (!expectedIsValid)
            {
                Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddressRequest.Street))); // Fixed: removed extra )
            }
        }

        [Theory]
        [InlineData("US", true)]
        [InlineData("CA", true)]
        [InlineData("XX", false)]
        [InlineData(null, false)]
        public void Country_Validation(string country, bool expectedIsValid)
        {
            var address = new AddressRequest
            {
                Street = "123 Valid St",
                City = "Valid City",
                PostalCode = "12345",
                Country = country
            };

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(address, 
                new ValidationContext(address), 
                results, 
                validateAllProperties: true);

            Assert.Equal(expectedIsValid, isValid);
            
            if (!expectedIsValid && country != null)
            {
                Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddressRequest.Country))); // Fixed: removed extra )
            }
        }
    }

    public static class StringExtensions
    {
        public static string Repeat(this string value, int count)
        {
            return string.Concat(Enumerable.Repeat(value, count));
        }
    }
}
