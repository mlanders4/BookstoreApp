using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("Ship_indDetails")] // Matches your exact table name
    public class ShippingEntity
    {
        [Key]
        [Column("ship_id")]
        public int ShippingId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("screw", TypeName = "varchar(100)")] // Assuming this is street address
        public string StreetAddress { get; set; }

        [Required]
        [Column("city", TypeName = "varchar(50)")]
        public string City { get; set; }

        [Required]
        [Column("zip", TypeName = "varchar(20)")]
        public string PostalCode { get; set; }

        [Required]
        [Column("country", TypeName = "varchar(50)")]
        public string Country { get; set; }

        [Column("shipping_method", TypeName = "varchar(30)")]
        public string ShippingMethod { get; set; } // Standard, Express, SameDay

        [Column("shipping_cost", TypeName = "decimal(10,2)")]
        public decimal ShippingCost { get; set; }

        [Column("estimated_delivery", TypeName = "varchar(50)")]
        public string EstimatedDelivery { get; set; }

        [Column("tracking_number", TypeName = "varchar(50)")]
        public string TrackingNumber { get; set; }

        [Column("shipped_date", TypeName = "datetime")]
        public DateTime? ShippedDate { get; set; }

        // Navigation property
        [ForeignKey("OrderId")]
        public OrderEntity Order { get; set; }

        // Status management
        public void MarkAsShipped(string trackingNum)
        {
            ShippedDate = DateTime.UtcNow;
            TrackingNumber = trackingNum;
        }

        public bool IsDelivered()
        {
            return ShippedDate.HasValue && 
                   ShippedDate.Value.AddDays(GetDeliveryDays()) < DateTime.UtcNow;
        }

        private int GetDeliveryDays()
        {
            return ShippingMethod switch
            {
                "Express" => 2,
                "SameDay" => 1,
                _ => 5 // Standard
            };
        }
    }
}
