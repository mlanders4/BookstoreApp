using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Checkout.Models.Requests
{
    public record CheckoutRequest(
        [Required]
        int UserId,  // Maps to Orders.user_id

        [Required]
        int CartId,  // Maps to Orders.cart_id

        [Required]
        PaymentInfoRequest Payment,  // Maps to Checkout table

        [Required]
        AddressRequest ShippingAddress,  // Maps to ShippingDetails

        // Optional fields from your schema
        string PromoCode = null,  // Could map to Sale.code
        List<CartItemRequest> Items = null  // For validation
    )
    {
        // Helper method to create Order entity
        public Dictionary<string, object> ToOrderDictionary()
        {
            return new Dictionary<string, object>
            {
                ["user_id"] = this.UserId,
                ["cart_id"] = this.CartId,
                ["status"] = "pending" // Default status
            };
        }

        // Validates cart items against inventory
        public bool ValidateItems(List<BookInventory> inventory)
        {
            if (Items == null) return true;
            
            foreach (var item in Items)
            {
                var book = inventory.Find(b => b.ISBN == item.BookId);
                if (book == null || book.Stock < item.Quantity)
                    return false;
            }
            return true;
        }
    }

    public record CartItemRequest(
        [Required]
        string BookId,  // Maps to CartItem.isbn

        [Range(1, 100)]
        int Quantity
    );

    public record BookInventory(string ISBN, int Stock);
}
