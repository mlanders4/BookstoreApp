using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Data.Entities
{
    public class Order
    {
        [Column("order_id")]
        public Guid Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("cart_id")] 
        public int CartId { get; set; }

        [Column("date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column("status")]
        public string Status { get; set; } = "pending"; // pending/processing/completed

        [Column("total_amount")]
        public decimal TotalAmount { get; set; }

        // Navigation properties
        public Payment Payment { get; set; }
        public ShippingDetail ShippingDetail { get; set; }
    }
}
