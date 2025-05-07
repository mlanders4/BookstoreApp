using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Data.Entities
{
    [Table("Orders")]
    public class Order
    {
        [Column("order_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("cart_id")] 
        public int CartId { get; set; }

        [Column("checkout_id")]
        public int CheckoutId { get; set; }

        [Column("date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;

        [Column("status")]
        public string Status { get; set; } = "pending";

        // Only include navigation properties you actually use
        public List<OrderItem> Items { get; set; } = new();
    }

    // Only include if you actually use order items
    [Table("OrderItems")]
    public class OrderItem
    {
        [Column("id")] // Adjust to match your actual PK column name
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("book_id")] // Or "isbn" if that's your column name
        public string BookId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }
    }
}
