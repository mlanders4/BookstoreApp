using Bookstore.Checkout.Models.Requests;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Bookstore.Checkout.Tests.Models.Requests
{
    public class AddressRequestTests
    {
        [Theory]
        [InlineData(null, false)] // Required field
        [InlineData("", false)]
        [InlineData("123 Main St", true)]
        [InlineData("A".Repeat(256), false)] // Exceeds max length
        public void Street_Validation(string street, bool expectedIsValid)
        {
            // Arrange
            var address = new AddressRequest(
                street: street,
                city: "Valid City",
                postalCode: "12345",
                country: "US");

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
        [InlineData("XX", false)] // Not in supported countries
        public void Country_Validation(string country, bool expectedIsValid)
        {
            var address = new AddressRequest(
                street: "123 Valid St",
                city: "Valid City",
                postalCode: "12345",
                country: country);

            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(address, new ValidationContext(address), results, true);

            Assert.Equal(expectedIsValid, isValid);
        }
    }

    // Helper extension for string repeating
    public static class StringExtensions
    {
        public static string Repeat
