using Bookstore.Checkout.Contracts;
using Bookstore.Checkout.Models.Requests;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Bookstore.Checkout.Engine;

public class PaymentValidator : IPaymentValidator
{
    private const int MinCardLength = 13; // Minimum for Visa/MC/Discover
    private const int MaxCardLength = 19; // Maximum for some cards
    private const int StandardCvvLength = 3;
    private const int AmexCvvLength = 4;
    
    // Regex for basic card number patterns (Luhn check comes later)
    private static readonly Regex CardNumberRegex = new(@"^\d{13,19}$", RegexOptions.Compiled);
    private static readonly Regex CvvRegex = new(@"^\d{3,4}$", RegexOptions.Compiled);

    public PaymentValidationResult Validate(PaymentInfoRequest payment)
    {
        // 1. Null check
        if (payment == null)
            return PaymentValidationResult.Fail("Payment information is required");

        // 2. Cardholder name validation
        if (string.IsNullOrWhiteSpace(payment.CardholderName?.Trim()))
            return PaymentValidationResult.Fail("Cardholder name is required");

        // 3. Card number validation
        var cardNumber = payment.CardNumber?.Replace(" ", "") ?? "";
        if (!CardNumberRegex.IsMatch(cardNumber))
            return PaymentValidationResult.Fail($"Card number must be {MinCardLength}-{MaxCardLength} digits");

        if (!PassesLuhnCheck(cardNumber))
            return PaymentValidationResult.Fail("Invalid card number");

        // 4. Expiry date validation
        if (!TryParseExpiryDate(payment.ExpiryDate, out var expiryDate))
            return PaymentValidationResult.Fail("Invalid expiry date (use MM/YY or MM/YYYY)");

        if (expiryDate < DateTime.Now.AddDays(-1)) // Grace period
            return PaymentValidationResult.Fail("Card has expired");

        // 5. CVV validation (dynamic length for Amex)
        var expectedCvvLength = IsAmericanExpress(cardNumber) ? AmexCvvLength : StandardCvvLength;
        
        if (string.IsNullOrWhiteSpace(payment.Cvv))
            return PaymentValidationResult.Fail("CVV is required");

        if (!CvvRegex.IsMatch(payment.Cvv) || payment.Cvv.Length != expectedCvvLength)
            return PaymentValidationResult.Fail($"CVV must be {expectedCvvLength} digits");

        return PaymentValidationResult.Success();
    }

    private bool TryParseExpiryDate(string input, out DateTime expiryDate)
    {
        expiryDate = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(input)) return false;

        try
        {
            var formats = new[] { "MM/yy", "MM/yyyy", "M/yy", "M/yyyy" };
            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, 
                DateTimeStyles.None, out var parsedDate))
            {
                expiryDate = new DateTime(parsedDate.Year, parsedDate.Month, 1)
                    .AddMonths(1)
                    .AddDays(-1);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool PassesLuhnCheck(string cardNumber)
    {
        // Luhn algorithm implementation
        int sum = 0;
        bool alternate = false;
        
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            if (!char.IsDigit(cardNumber[i])) return false;
            
            int digit = cardNumber[i] - '0';
            if (alternate)
            {
                digit *= 2;
                if (digit > 9) digit -= 9;
            }
            sum += digit;
            alternate = !alternate;
        }
        return sum % 10 == 0;
    }

    private bool IsAmericanExpress(string cardNumber)
    {
        // Amex cards start with 34 or 37 and are 15 digits
        return cardNumber.Length == 15 && 
               (cardNumber.StartsWith("34") || cardNumber.StartsWith("37"));
    }
}
