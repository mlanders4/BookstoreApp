using System;
using System.Collections.Generic;

namespace Bookstore.Checkout.Models.Responses
{
    public record CheckoutResponse(
        bool IsSuccess,
        string OrderId = null,
        string OrderStatus = null,       // Maps to Orders.status
        decimal? OrderTotal = null,      // From Checkout.amount
        DateTime? EstimatedDelivery = null,
        PaymentResult Payment = null,    // From Checkout table
        ShippingSummary Shipping = null, // From ShippingDetails
        List<CheckoutError> Errors = null
    )
    {
        // Successful checkout
        public static CheckoutResponse Success(
            string orderId,
            decimal orderTotal,
            string orderStatus,
            PaymentResult payment,
            ShippingSummary shipping)
        {
            return new CheckoutResponse(
                IsSuccess: true,
                OrderId: orderId,
                OrderStatus: orderStatus,
                OrderTotal: orderTotal,
                Payment: payment,
                Shipping: shipping,
                EstimatedDelivery: CalculateDeliveryDate(shipping)
            );
        }

        // Failed checkout
        public static CheckoutResponse Failure(
            List<CheckoutError> errors,
            string partialOrderId = null,
            string failedStatus = "failed")
        {
            return new CheckoutResponse(
                IsSuccess: false,
                OrderId: partialOrderId,
                OrderStatus: failedStatus,
                Errors: errors
            );
        }

        // Single error convenience method
        public static CheckoutResponse FromError(string errorMessage, string errorCode)
        {
            return new CheckoutResponse(
                IsSuccess: false,
                Errors: new List<CheckoutError> { new(errorMessage, errorCode) }
            );
        }

        // Database-ready format
        public Dictionary<string, object> ToOrderDictionary()
        {
            return new Dictionary<string, object>
            {
                ["order_id"] = this.OrderId,
                ["status"] = this.OrderStatus,
                ["total"] = this.OrderTotal,
                ["payment_status"] = this.Payment?.Status
            };
        }

        private static DateTime? CalculateDeliveryDate(ShippingSummary shipping)
        {
            if (shipping == null) return null;
            
            return shipping.Method switch
            {
                "Standard" => DateTime.UtcNow.AddDays(5),
                "Express" => DateTime.UtcNow.AddDays(2),
                "Overnight" => DateTime.UtcNow.AddDays(1),
                _ => null
            };
        }
    }

    // Supporting records
    public record PaymentResult(
        string TransactionId,     // From Checkout.checkout_id
        string MaskedCardNumber,  // From Checkout.credit_card_number
        string Status            // "approved", "declined", etc.
    );

    public record ShippingSummary(
        string Carrier,          // From ShippingDetails carrier
        string Method,           // From ShippingDetails method
        decimal Cost,            // From ShippingDetails cost
        string TrackingNumber    // Will be added when shipped
    );

    public record CheckoutError(
        string Message,
        string Code,             // "payment_failed", "out_of_stock", etc.
        string Details = null
    );
}
