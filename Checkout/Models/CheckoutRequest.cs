namespace Bookstore.Checkout.Models;

public record CheckoutRequest(
    string UserId,                  // User placing the order
    List<CartItem> Items,           // Books in cart
    PaymentInfo Payment,            // Credit card details
    Address ShippingAddress,        // Delivery address
    ShippingMethod ShippingMethod   // Standard/Express/SameDay
);
