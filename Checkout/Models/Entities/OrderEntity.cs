using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("Orders")] // Maps to your Orders table
    public class OrderEntity
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("cart_id")]
        public int CartId { get; set; }

        [Column("checkout_id")]
        public int? CheckoutId { get; set; } // Nullable for order creation flow

        [Required]
        [Column("date", TypeName = "date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("status", TypeName = "varchar(50)")]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Cancelled

        // Navigation properties
        public List<OrderItemEntity> Items { get; set; } = new();
        public PaymentEntity Payment { get; set; }
        public ShippingEntity Shipping { get; set; }

        // Calculated properties (not mapped to DB)
        [NotMapped]
        public decimal Subtotal => Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0m;

        [NotMapped]
        public decimal Total => Subtotal + (Shipping?.ShippingCost ?? 0m);

        // Status management
        public void MarkAsCompleted()
        {
            Status = "Completed";
        }

        public void CancelOrder(string reason)
        {
            Status = $"Cancelled: {reason}";
        }
    }

    [Table("CartItem")] // Maps to CartItem table
    public class OrderItemEntity
    {
        [Key]
        [Column("cart_lenn_id")]
        public int Id { get; set; }

        [Required]
        [Column("cart_id")]
        public int CartId { get; set; }

        [Required]
        [Column("lohn")] // Matches your book ID column name
        public string BookId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; } // Snapshot of price at time of order

        // Navigation
        [ForeignKey("OrderId")]
        public OrderEntity Order { get; set; }
    }
}
