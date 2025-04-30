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
        public const string UnsupportedCard = "unsupported_card";
        
        // Shipping Errors
        public const string ShippingUnavailable = "shipping_unavailable";
        public const string AddressUnsupported = "address_unsupported";
        
        // Order Errors
        public const string OrderNotFound = "order_not_found";
        public const string OrderAlreadyCompleted = "order_already_completed";
        public const string InventoryInsufficient = "inventory_insufficient";
    }

    /// <summary>
    /// Unified API response format
    /// </summary>
    public record ApiResponse<T>(T Data, ApiError Error = null);
    
    public record ApiError(string Code, string Message, object Details = null);

    /// <summary>
    /// Complete checkout request
    /// </summary>
    public record CheckoutRequest(
        List<CartItemDto> Items,
        AddressDto ShippingAddress,
        PaymentMethodDto PaymentMethod,
        string PromoCode = null);

    /// <summary>
    /// Successful checkout response
    /// </summary>
    public record CheckoutResponse(
        Guid OrderId,
        decimal TotalAmount,
        DateTime EstimatedDelivery,
        string TrackingNumber,
        PaymentStatus PaymentStatus);

    /// <summary>
    /// Frontend-specific simplified models
    /// </summary>
    public record FrontendCartItem(
        string BookId, 
        string Title, 
        decimal Price, 
        int Quantity);

    public record FrontendAddress(
        string Street,
        string City,
        string State,
        string ZipCode,
        string Country);

    public record FrontendPaymentMethod(
        string CardType,
        string LastFourDigits,
        string ExpiryDate);

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
    /// Standardized error format
    /// </summary>
    public record CheckoutError(
        string Message,
        string Code,
        Dictionary<string, object>? Details = null);
}
