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

        // Navigation properties
        public virtual Payment Payment { get; set; }
        public virtual ShippingDetail ShippingDetail { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    [Table("OrderItems")]
    public class OrderItem
    {
        [Column("order_item_id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("isbn")]
        public string BookId { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("unit_price")]
        public decimal UnitPrice { get; set; }

        public virtual Order Order { get; set; }
    }
}
