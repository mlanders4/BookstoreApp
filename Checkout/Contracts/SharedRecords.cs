using System;
using System.Collections.Generic;

namespace Bookstore.Checkout.Contracts
{
    /// <summary>
    /// Standardized error codes and shared DTOs for the Checkout domain
    /// </summary>
    public static class ErrorCodes
    {
        // Payment Errors
        public const string PaymentInvalid = "payment_invalid";
        public const string PaymentExpired = "payment_expired";
        public const string UnsupportedCard = "unsupported_card";
        
        // Shipping Errors
        public const string ShippingUnavailable = "shipping_unavailable";
        public const string AddressUnsupported = "address_unsupported";
        
        // Order Errors
        public const string OrderNotFound = "order_not_found";
        public const string OrderAlreadyCompleted = "order_already_completed";
        public const string InventoryInsufficient = "inventory_insufficient";
        
        // System Errors
        public const string ServerError = "server_error";
        public const string DatabaseError = "database_error";
    }

    /// <summary>
    /// Standard API response format
    /// </summary>
    public record ApiResponse<T>(T Data, ApiError Error = null);

    /// <summary>
    /// Standardized error payload
    /// </summary>
    public record ApiError(string Code, string Message, object Details = null);

    /// <summary>
    /// Complete checkout request
    /// </summary>
    public record CheckoutRequest(
        int UserId,
        int CartId,
        List<CartItemDto> Items,
        AddressDto ShippingAddress,
        PaymentMethodDto PaymentMethod,
        string PromoCode = null);

    /// <summary>
    /// Successful checkout response
    /// </summary>
    public record CheckoutResponse(
        int OrderId,
        decimal TotalAmount,
        DateTime EstimatedDelivery,
        string TrackingNumber,
        PaymentStatus PaymentStatus);

    /// <summary>
    /// Cart item details
    /// </summary>
    public record CartItemDto(
        string BookId,
        string Title,
        decimal Price,
        int Quantity);

    /// <summary>
    /// Shipping address details
    /// </summary>
    public record AddressDto(
        string Street,
        string City,
        string State,
        string ZipCode,
        string Country)
    {
        public string ToSingleLine() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
    }

    /// <summary>
    /// Payment method details
    /// </summary>
    public record PaymentMethodDto(
        string CardType,
        string CardNumber,
        string ExpiryDate,
        string Cvv,
        string CardholderName);

    /// <summary>
    /// Shipping status update request
    /// </summary>
    public record ShippingStatusUpdateRequest(string Status);

    /// <summary>
    /// Payment validation result
    /// </summary>
    public record PaymentValidationResult(
        bool IsValid, 
        string ErrorMessage = null, 
        Dictionary<string, object> Details = null)
    {
        public static PaymentValidationResult Success() => new(true);
        public static PaymentValidationResult Fail(string error, Dictionary<string, object> details = null) 
            => new(false, error, details);
    }

    /// <summary>
    /// Shipping options response
    /// </summary>
    public record ShippingOptionsResponse(
        string MethodName,
        decimal Cost,
        string DeliveryEstimate,
        string Carrier = "USPS",
        bool IsAvailable = true);

    /// <summary>
    /// Payment status enumeration
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
    /// Shipping method enumeration
    /// </summary>
    public enum ShippingMethod
    {
        Standard,
        Express,
        Overnight
    }

    /// <summary>
    /// Standardized checkout exception
    /// </summary>
    public class CheckoutException : Exception
    {
        public string ErrorCode { get; }
        public Dictionary<string, object> Details { get; }

        public CheckoutException(
            string errorCode, 
            string message = null, 
            Dictionary<string, object> details = null)
            : base(message ?? $"Checkout error: {errorCode}")
        {
            ErrorCode = errorCode;
            Details = details ?? new Dictionary<string, object>();
        }
    }
}
