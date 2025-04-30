using Bookstore.Checkout.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Bookstore.Checkout.Core.Services
{
    public class PaymentValidator : IPaymentValidator
    {
        private readonly ILogger<PaymentValidator> _logger;
        private readonly IConfiguration _config;
        private static readonly Regex _cardNumberRegex = new(@"^\d{13,19}$", RegexOptions.Compiled);
        private static readonly Regex _cvvRegex = new(@"^\d{3,4}$", RegexOptions.Compiled);
        private static readonly Regex _expiryRegex = new(@"^(0[1-9]|1[0-2])\/?([0-9]{2,4})$", RegexOptions.Compiled);

        public PaymentValidator(ILogger<PaymentValidator> logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public PaymentValidationResult Validate(PaymentMethodDto payment)
        {
            if (payment == null)
            {
                _logger.LogWarning("Null payment request received");
                return PaymentValidationResult.Fail(
                    ErrorCodes.PaymentInvalid, 
                    "Payment information is required");
            }

            var errors = new List<CheckoutError>();
            ValidateCardNumber(payment.CardNumber, errors);
            ValidateCardholderName(payment.CardholderName, errors);
            ValidateExpiryDate(payment.ExpiryDate, errors);
            ValidateCvv(payment.Cvv, payment.CardNumber, errors);
            ValidateCountryRestrictions(payment.BillingAddress?.Country, errors);
            ValidateCardType(payment.CardNumber, errors);

            if (errors.Count > 0)
            {
                _logger.LogWarning("Payment validation failed with {ErrorCount} errors", errors.Count);
                return PaymentValidationResult.Fail(
                    ErrorCodes.PaymentInvalid,
                    "Payment validation failed",
                    new Dictionary<string, object> { ["errors"] = errors });
            }

            _logger.LogInformation("Payment validated successfully for card ending {Last4}", 
                payment.CardNumber[^4..]);
            return PaymentValidationResult.Success();
        }

        private void ValidateCardNumber(string cardNumber, List<CheckoutError> errors)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Card number is required"));
                return;
            }

            var cleanNumber = cardNumber.Replace(" ", "");
            
            if (!_cardNumberRegex.IsMatch(cleanNumber))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Card number must be 13-19 digits"));
                return;
            }

            if (!PassesLuhnCheck(cleanNumber))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Invalid card number"));
            }
        }

        private void ValidateCardType(string cardNumber, List<CheckoutError> errors)
        {
            var cardType = DetectCardType(cardNumber);
            var allowedCards = _config.GetSection("Payment:AcceptedCardTypes").Get<string[]>();
            
            if (allowedCards == null || !allowedCards.Contains(cardType.ToString()))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.UnsupportedCard,
                    $"We don't accept {cardType} cards"));
            }
        }

        private void ValidateCardholderName(string name, List<CheckoutError> errors)
        {
            if (string.IsNullOrWhiteSpace(name?.Trim()))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Cardholder name is required"));
                return;
            }

            if (name.Length < 2 || name.Length > 100)
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Cardholder name must be 2-100 characters"));
            }

            if (Regex.IsMatch(name, @"\d"))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Cardholder name cannot contain numbers"));
            }
        }

        private void ValidateExpiryDate(string expiryDate, List<CheckoutError> errors)
        {
            if (string.IsNullOrWhiteSpace(expiryDate))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Expiry date is required"));
                return;
            }

            if (!_expiryRegex.IsMatch(expiryDate))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Invalid expiry date format (use MM/YY or MM/YYYY)"));
                return;
            }

            if (!TryParseExpiryDate(expiryDate, out var expiryDateParsed))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "Invalid expiry date"));
                return;
            }

            if (expiryDateParsed < DateTime.Now.AddDays(-5)) // 5-day grace period
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentExpired,
                    "Card has expired",
                    new Dictionary<string, object>
                    {
                        ["expiryDate"] = expiryDateParsed.ToString("MM/yyyy")
                    }));
            }
        }

        private void ValidateCvv(string cvv, string cardNumber, List<CheckoutError> errors)
        {
            if (string.IsNullOrWhiteSpace(cvv))
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    "CVV is required"));
                return;
            }

            var expectedLength = string.IsNullOrEmpty(cardNumber) 
                ? 3 
                : IsAmericanExpress(cardNumber) ? 4 : 3;

            if (!_cvvRegex.IsMatch(cvv) || cvv.Length != expectedLength)
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    $"CVV must be {expectedLength} digits"));
            }
        }

        private void ValidateCountryRestrictions(string country, List<CheckoutError> errors)
        {
            if (string.IsNullOrWhiteSpace(country)) return;

            var blockedCountries = _config.GetSection("Payment:BlockedCountries").Get<string[]>();
            if (blockedCountries?.Contains(country, StringComparer.OrdinalIgnoreCase) == true)
            {
                errors.Add(CheckoutError.Create(
                    ErrorCodes.PaymentInvalid,
                    $"We cannot accept payments from {country}",
                    new Dictionary<string, object> { ["country"] = country }));
            }
        }

        private static bool TryParseExpiryDate(string input, out DateTime expiryDate)
        {
            expiryDate = DateTime.MinValue;
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

        private static bool PassesLuhnCheck(string cardNumber)
        {
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

        private static bool IsAmericanExpress(string cardNumber)
        {
            return cardNumber.Length == 15 && 
                   (cardNumber.StartsWith("34") || cardNumber.StartsWith("37"));
        }

        private static CardType DetectCardType(string cardNumber)
        {
            if (cardNumber.StartsWith("4")) return CardType.Visa;
            if (cardNumber.StartsWith("5")) return CardType.Mastercard;
            if (cardNumber.StartsWith("34") || cardNumber.StartsWith("37")) return CardType.AmericanExpress;
            if (cardNumber.StartsWith("6")) return CardType.Discover;
            return CardType.Unknown;
        }

        private enum CardType
        {
            Unknown,
            Visa,
            Mastercard,
            AmericanExpress,
            Discover
        }
    }
}
