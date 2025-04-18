using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Checkout.Models.Requests
{
    public record PaymentInfoRequest(
        [Required(ErrorMessage = "Card number is required")]
        [CreditCard(ErrorMessage = "Invalid card number format")]
        string CardNumber,

        [Required(ErrorMessage = "Cardholder name is required")]
        [StringLength(100, ErrorMessage = "Name too long")]
        string CardholderName,

        [Required(ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2}|[0-9]{4})$", 
            ErrorMessage = "Use MM/YY or MM/YYYY format")]
        string ExpiryDate,

        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3-4 digits")]
        string Cvv
    )
    {
        public DateTime GetParsedExpiryDate()
        {
            if (string.IsNullOrWhiteSpace(ExpiryDate))
                throw new ArgumentException("Expiry date is empty");

            var parts = ExpiryDate.Split('/');
            if (parts.Length != 2)
                throw new FormatException("Invalid expiry date format");

            if (!int.TryParse(parts[0], out var month) || month < 1 || month > 12)
                throw new ArgumentOutOfRangeException("Invalid month");

            if (!int.TryParse(parts[1], out var year))
                throw new FormatException("Invalid year");

            // Handle 2-digit year
            year = year < 100 ? 2000 + year : year;

            return new DateTime(year, month, 1)
                .AddMonths(1) // Last day of expiry month
                .AddDays(-1);
        }

        public string GetMaskedCardNumber()
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || CardNumber.Length < 4)
                return "****";

            return $"****-****-****-{CardNumber[^4..]}";
        }
    }
}
