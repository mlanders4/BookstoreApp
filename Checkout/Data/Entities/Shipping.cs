using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("ShippingDetails")] // Explicitly maps to your table
    public class ShippingEntity
    {
        [Key]
        [Column("ship_id")]
        public int ShippingId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("street", TypeName = "varchar(255)")] // Matches your schema
        public string StreetAddress { get; set; }

        [Required]
        [Column("city", TypeName = "varchar(100)")]
        public string City { get; set; }

        [Required]
        [Column("zip", TypeName = "varchar(20)")]
        public string PostalCode { get; set; }

        [Required]
        [Column("country", TypeName = "varchar(100)")]
        public string Country { get; set; }

        // Business logic properties (not mapped to DB)
        [NotMapped]
        public decimal ShippingCost { get; set; }

        [NotMapped]
        public string ShippingMethod { get; set; }

        [NotMapped]
        public DateTime? EstimatedDelivery { get; set; }

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual OrderEntity Order { get; set; }

        // Status tracking methods
        public void MarkAsShipped(string carrier, string trackingNumber)
        {
            // These would be stored in your business logic layer
            ShippingMethod = carrier;
            EstimatedDelivery = CalculateDeliveryDate();
        }

        private DateTime CalculateDeliveryDate()
        {
            return Country.Equals("USA", StringComparison.OrdinalIgnoreCase)
                ? DateTime.UtcNow.AddDays(3)  // Domestic
                : DateTime.UtcNow.AddDays(10); // International
        }

        // Conversion from AddressRequest
        public static ShippingEntity FromAddressRequest(int orderId, AddressRequest address)
        {
            return new ShippingEntity
            {
                OrderId = orderId,
                StreetAddress = address.Street,
                City = address.City,
                PostalCode = address.PostalCode,
                Country = address.Country
            };
        }
    }
}
