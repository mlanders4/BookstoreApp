using System;
using System.Collections.Generic;

namespace Bookstore.Checkout.Contracts
{
    /// <summary>
    /// Standardized error codes for the checkout domain
    /// </summary>
    public static class ErrorCodes
    {
        // Payment Errors
        public const string PaymentInvalid = "payment_invalid";
        public const string PaymentExpired = "payment_expired";
        public const string PaymentProcessorDown = "payment_processor_unavailable";
        
        // Shipping Errors
        public const string ShippingUnavailable = "shipping_unavailable";
        public const string AddressUnsupported = "address_unsupported";
        
        // Order Errors
        public const string OrderNotFound = "order_not_found";
        public const string OrderAlreadyCompleted = "order_already_completed";
        public const string InventoryInsufficient = "inventory_insufficient";
        
        // System Errors
        public const string DatabaseUnavailable = "database_unavailable";
        public const string ExternalServiceFailure = "external_service_failure";
    }

    /// <summary>
    /// Supported shipping methods with business constraints
    /// </summary>
    public enum ShippingMethod 
    {
        Standard = 1,    // 3-5 business days
        Express = 2,     // 1-2 business days
        SameDay = 3,     // Same day (if ordered before 12PM)
        International = 4 // 7-14 business days
    }

    /// <summary>
    /// Result of checkout validation phase
    /// </summary>
    public record CheckoutValidationResult(
        bool IsValid,
        List<CheckoutError>? Errors = null,
        decimal? CalculatedShippingCost = null,
        DateTime? EstimatedDelivery = null,
        string? ShippingCarrier = null)
    {
        public static CheckoutValidationResult Success(
            decimal shippingCost, 
            DateTime estimate,
            string carrier) 
            => new(true, null, shippingCost, estimate, carrier);

        public static CheckoutValidationResult Fail(List<CheckoutError> errors) 
            => new(false, errors);
        
        public static CheckoutValidationResult FromSingleError(string message, string code)
            => new(false, new List<CheckoutError> { new(message, code) });
    }

    /// <summary>
    /// Result of payment validation
    /// </summary>
    public record PaymentValidationResult(
        bool IsValid,
        string? ErrorCode = null,
        string? ErrorMessage = null,
        Dictionary<string, object>? Metadata = null)
    {
        public static PaymentValidationResult Success() 
            => new(true);
        
        public static PaymentValidationResult Fail(string code, string message) 
            => new(false, code, message);
        
        public static PaymentValidationResult FailWithMetadata(
            string code, 
            string message,
            Dictionary<string, object> metadata)
            => new(false, code, message, metadata);
    }

    /// <summary>
    /// Standardized error format
    /// </summary>
    public record CheckoutError(
        string Message,
        string Code,
        Dictionary<string, object>? Details = null)
    {
        public static CheckoutError Create(
            string code,
            string message,
            Dictionary<string, object>? details = null)
            => new(message, code, details);
    }

    /// <summary>
    /// Shipping calculation response
    /// </summary>
    public record ShippingOptionsResponse(
        ShippingMethod Method,
        decimal Cost,
        DateTime EstimatedDelivery,
        string Carrier,
        bool IsAvailable = true,
        string[]? ServiceOptions = null,
        CheckoutError? Error = null)
    {
        public static ShippingOptionsResponse CreateError(
            ShippingMethod method,
            string errorCode,
            string errorMessage)
            => new(
                method, 
                0, 
                DateTime.MinValue, 
                "", 
                false, 
                null, 
                CheckoutError.Create(errorCode, errorMessage));
    }

    /// <summary>
    /// Payment processing statuses
    /// </summary>
    public enum PaymentStatus
    {
        Pending,
        Authorized,
        Completed,
        Failed,
        Refunded
    }

    /// <summary>
    /// Order lifecycle states
    /// </summary>
    public enum OrderStatus
    {
        Draft,
        PendingPayment,
        Processing,
        Shipped,
        Delivered,
        Cancelled,
        Returned
    }

    /// <summary>
    /// Address validation rules
    /// </summary>
    public static class AddressRules
    {
        public static readonly string[] SupportedCountries = 
        { 
            "US", "CA", "GB", "AU", "DE" 
        };

        public static bool IsSupportedCountry(string countryCode)
            => Array.Exists(SupportedCountries, 
                x => x.Equals(countryCode, StringComparison.OrdinalIgnoreCase));
    }
}
