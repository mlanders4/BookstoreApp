using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("Orders")]
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

        [Column("checkout_id")]
        public int? CheckoutId { get; set; }

        [Required]
        [Column("date", TypeName = "datetime2")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("status", TypeName = "varchar(20)")]
        public string Status { get; set; } = OrderStatus.Pending;

        [Column("total_amount", TypeName = "decimal(10,2)")]
        public decimal? TotalAmount { get; set; }

        // Navigation properties
        [ForeignKey("CheckoutId")]
        public virtual PaymentEntity Payment { get; set; }

        [ForeignKey("OrderId")]
        public virtual ICollection<OrderItemEntity> Items { get; set; } = new List<OrderItemEntity>();

        [ForeignKey("OrderId")]
        public virtual ShippingEntity Shipping { get; set; }

        // Business logic methods
        public decimal CalculateTotal()
        {
            if (Items == null || Items.Count == 0)
                return 0;

            return Items.Sum(i => i.UnitPrice * i.Quantity) + (Shipping?.ShippingCost ?? 0);
        }

        public bool CanTransitionTo(string newStatus)
        {
            return (Status, newStatus) switch
            {
                (OrderStatus.Pending, OrderStatus.Processing) => true,
                (OrderStatus.Processing, OrderStatus.Shipped) => true,
                (OrderStatus.Shipped, OrderStatus.Completed) => true,
                (_, OrderStatus.Cancelled) => Status != OrderStatus.Completed,
                _ => false
            };
        }
    }

    [Table("CartItem")]
    public class OrderItemEntity
    {
        [Key]
        [Column("cart_item_id")]
        public int CartItemId { get; set; }

        [Required]
        [Column("cart_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("isbn", TypeName = "varchar(20)")]
        public string BookId { get; set; }

        [Required]
        [Column("quantity")]
        public int Quantity { get; set; }

        [Required]
        [Column("unit_price", TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        // Navigation back to order
        [ForeignKey("OrderId")]
        public virtual OrderEntity Order { get; set; }
    }

    public static class OrderStatus
    {
        public const string Pending = "pending";
        public const string Processing = "processing";
        public const string Shipped = "shipped";
        public const string Completed = "completed";
        public const string Cancelled = "cancelled";
    }

    public class OrderValidationException : Exception
    {
        public OrderValidationException(string message, Exception innerException = null) 
            : base(message, innerException) { }
    }
}
