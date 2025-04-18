using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("Checkout")] // Maps to your Checkout table
    public class PaymentEntity
    {
        [Key]
        [Column("checkout_id")]
        public int CheckoutId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("credit_card_number", TypeName = "varchar(50)")]
        public string MaskedCardNumber { get; set; } // Stores only last 4 digits + mask

        [Required]
        [Column("expiry_date", TypeName = "date")]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column("payment_date", TypeName = "datetime")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [Column("payment_status", TypeName = "varchar(20)")]
        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        // Navigation property
        [ForeignKey("OrderId")]
        public OrderEntity Order { get; set; }

        // Helper methods
        public void MaskCardNumber(string fullCardNumber)
        {
            if (string.IsNullOrWhiteSpace(fullCardNumber) return;
            
            // Store only last 4 digits (e.g., ****-****-****-4242)
            MaskedCardNumber = $"****-****-****-{fullCardNumber[^4..]}";
        }

        public void ProcessPayment(bool isSuccessful)
        {
            Status = isSuccessful ? "Completed" : "Failed";
            PaymentDate = DateTime.UtcNow;
        }

        public bool IsCardExpired()
        {
            return ExpiryDate < DateTime.Now.AddMonths(-1); // 1 month grace period
        }
    }
}
