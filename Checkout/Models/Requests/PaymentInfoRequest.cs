using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Bookstore.Checkout.Models.Requests
{
    public record PaymentInfoRequest(
        [Required(ErrorMessage = "Card number is required")]
        [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Invalid card number format")]
        string CardNumber,

        [Required(ErrorMessage = "Cardholder name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2-100 characters")]
        string CardholderName,

        [Required(ErrorMessage = "Expiry date is required")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", 
            ErrorMessage = "Use MM/YY format")]
        string ExpiryDate,

        [Required(ErrorMessage = "CVV is required")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3-4 digits")]
        string Cvv,

        // Optional for your Checkout table
        decimal? Amount = null
    )
    {
        public DateTime GetExpiryDateTime()
        {
            var parts = ExpiryDate.Split('/');
            if (parts.Length != 2 || !int.TryParse(parts[1], out var year))
                throw new ArgumentException("Invalid expiry date format");

            year = 2000 + year; // Convert YY to YYYY
            if (!int.TryParse(parts[0], out var month) || month < 1 || month > 12)
                throw new ArgumentException("Invalid month");

            return new DateTime(year, month, 1)
                .AddMonths(1) // Last day of expiry month
                .AddDays(-1);
        }

        public string GetMaskedCardNumber()
            => $"****-****-****-{CardNumber[^4..]}";

        public Dictionary<string, object> ToCheckoutTable(int orderId)
        {
            return new Dictionary<string, object>
            {
                ["order_id"] = orderId,
                ["credit_card_number"] = GetMaskedCardNumber(),
                ["expiry_date"] = GetExpiryDateTime(),
                ["amount"] = Amount
            };
        }
    }
}
