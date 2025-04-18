using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Requests;
using System.Globalization;

namespace Bookstore.Checkout.Engine;

public class PaymentValidator : IPaymentValidator
{
    private const int MinCardLength = 13;
    private const int MaxCardLength = 19;
    private const int CvvLength = 3; // 4 for Amex

    public PaymentValidationResult Validate(PaymentInfoRequest payment)
    {
        // 1. Null check
        if (payment == null)
            return PaymentValidationResult.Fail("Payment information is required");

        // 2. Card number validation
        var cardNumber = payment.CardNumber?.Replace(" ", "") ?? "";
        if (cardNumber.Length < MinCardLength || cardNumber.Length > MaxCardLength)
            return PaymentValidationResult.Fail($"Card number must be {MinCardLength}-{MaxCardLength} digits");
        
        if (!cardNumber.All(char.IsDigit))
            return PaymentValidationResult.Fail("Card number must contain only digits");

        // 3. Cardholder name validation
        if (string.IsNullOrWhiteSpace(payment.CardholderName))
            return PaymentValidationResult.Fail("Cardholder name is required");

        // 4. Expiry date validation
        if (!TryParseExpiryDate(payment.ExpiryDate, out var expiryDate))
            return PaymentValidationResult.Fail("Invalid expiry date format (MM/YYYY or MM/YY)");

        if (expiryDate < DateTime.Now.AddDays(-1)) // Allow grace period
            return PaymentValidationResult.Fail("Card has expired");

        // 5. CVV validation
        if (string.IsNullOrWhiteSpace(payment.Cvv))
            return PaymentValidationResult.Fail("CVV is required");

        if (payment.Cvv.Length != CvvLength || !payment.Cvv.All(char.IsDigit))
            return PaymentValidationResult.Fail($"CVV must be {CvvLength} digits");

        return PaymentValidationResult.Success();
    }

    private bool TryParseExpiryDate(string input, out DateTime expiryDate)
    {
        expiryDate = DateTime.MinValue;
        
        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Support both MM/YYYY and MM/YY formats
        var parts = input.Split('/');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out var month) || month < 1 || month > 12)
            return false;

        if (!int.TryParse(parts[1], out var year))
            return false;

        // Handle 2-digit year (assume 2000+)
        if (year < 100)
            year += 2000;

        try
        {
            expiryDate = new DateTime(year, month, 1)
                .AddMonths(1) // Last day of expiry month
                .AddDays(-1);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
