using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("Orders")] // Explicitly maps to your Orders table
    public class OrderEntity
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("cart_id")]
        public int CartId { get; set; }

        [Required]
        [Column("checkout_id")]
        public int CheckoutId { get; set; }

        [Required]
        [Column("date", TypeName = "date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;

        [Required]
        [Column("status", TypeName = "varchar(50)")]
        public string Status { get; set; } = "pending"; // Matches your status lifecycle

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual UserEntity User { get; set; }

        [ForeignKey("CartId")]
        public virtual CartEntity Cart { get; set; }

        [ForeignKey("CheckoutId")]
        public virtual PaymentEntity Payment { get; set; }

        public virtual ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();

        public virtual ShippingEntity Shipping { get; set; }

        // Business logic methods
        public void UpdateStatus(string newStatus)
        {
            var validStatuses = new[] { "pending", "processing", "shipped", "cancelled", "completed" };
            if (!validStatuses.Contains(newStatus.ToLower()))
                throw new ArgumentException("Invalid order status");

            Status = newStatus.ToLower();

            if (newStatus == "shipped")
                Shipping?.MarkAsShipped();
        }

        // Calculated properties
        [NotMapped]
        public decimal Subtotal => Items?.Sum(i => i.UnitPrice * i.Quantity) ?? 0m;

        [NotMapped]
        public decimal Total => Subtotal + (Shipping?.ShippingCost ?? 0m);

        // Conversion from checkout request
        public static OrderEntity FromCheckoutRequest(CheckoutRequest request, int checkoutId)
        {
            return new OrderEntity
            {
                UserId = request.UserId,
                CartId = request.CartId,
                CheckoutId = checkoutId,
                Status = "pending",
                OrderDate = DateTime.UtcNow.Date
            };
        }
    }

    [Table("CartItem")] // Maps to CartItem table
    public class OrderItemEntity
    {
        [Key]
        [Column("cart_item_id")]
        public int Id { get; set; }

        [Required]
        [Column("cart_id")]
        public int CartId { get; set; }

        [Required]
        [Column("isbn", TypeName = "varchar(20)")]
        public string BookId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; } // Snapshot of price at time of order

        [ForeignKey("CartId")]
        public virtual OrderEntity Order { get; set; }
    }
}
