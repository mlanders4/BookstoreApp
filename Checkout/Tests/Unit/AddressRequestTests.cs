using Bookstore.Checkout.Models.Requests;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Xunit;

namespace Bookstore.Checkout.Tests.Unit
{
    public class AddressRequestTests
    {
        [Theory]
        [InlineData(null, false)] // Required
        [InlineData("", false)]
        [InlineData("123 Main St", true)]
        [InlineData("A".Repeat(256), false)] // Exceeds max length
        public void Street_Validation(string street, bool expectedIsValid)
        {
            var address = new AddressRequest(
                street,
                "Lincoln",
                "68508",
                "US");

            var results = ValidateModel(address);
            Assert.Equal(expectedIsValid, !ContainsError(results, "Street"));
        }

        [Theory]
        [InlineData("USA", true)]
        [InlineData("Canada", true)]
        [InlineData("Japan", false)] // Not in supported countries
        public void IsSupportedCountry_ValidatesCorrectly(string country, bool expectedSupported)
        {
            var address = new AddressRequest(
                "123 Main St",
                "Lincoln",
                "68508",
                country);

            Assert.Equal(expectedSupported, address.IsSupportedCountry());
        }

        [Fact]
        public void ToShippingDetails_ConvertsCorrectly()
        {
            var address = new AddressRequest(
                "123 Main St",
                "Lincoln",
                "68508",
                "US");

            var details = address.ToShippingDetails(1001);

            Assert.Equal(1001, details["order_id"]);
            Assert.Equal("123 Main St", details["street"]);
            Assert.Equal("Lincoln", details["city"]);
            Assert.Equal("68508", details["zip"]);
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }

        private static bool ContainsError(List<ValidationResult> results, string memberName)
        {
            return results.Exists(v => v.MemberNames.Contains(memberName));
        }
    }

    // Extension method for string repeat
    internal static class StringExtensions
    {
        public static string Repeat(this string value, int count)
        {
            return string.Concat(System.Linq.Enumerable.Repeat(value, count));
        }
    }
}
