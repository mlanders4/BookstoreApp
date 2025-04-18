using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Models.Entities
{
    [Table("Checkout")] // Explicitly maps to your Checkout table
    public class PaymentEntity
    {
        [Key]
        [Column("checkout_id")]
        public int CheckoutId { get; set; }

        [Required]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("credit_card_number", TypeName = "varchar(20)")]
        public string MaskedCardNumber { get; set; } // Stores only last 4 digits + mask

        [Required]
        [Column("expiry_date", TypeName = "date")]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Column("amount", TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        [Column("payment_date", TypeName = "datetime")]
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        // Business logic status (not mapped to DB)
        [NotMapped]
        public string PaymentStatus { get; set; } // "Pending", "Completed", "Failed"

        // Navigation property
        [ForeignKey("OrderId")]
        public virtual OrderEntity Order { get; set; }

        // Constructor for creating from payment request
        public static PaymentEntity FromPaymentRequest(int orderId, PaymentInfoRequest request, decimal amount)
        {
            return new PaymentEntity
            {
                OrderId = orderId,
                MaskedCardNumber = request.GetMaskedCardNumber(),
                ExpiryDate = request.GetExpiryDateTime(),
                Amount = amount,
                PaymentStatus = "Pending"
            };
        }

        // Payment processing result
        public void ProcessPaymentResult(bool isSuccessful, string transactionId = null)
        {
            PaymentStatus = isSuccessful ? "Completed" : "Failed";
            PaymentDate = DateTime.UtcNow;
            
            if (isSuccessful && !string.IsNullOrEmpty(transactionId))
            {
                // Store transaction ID if needed
            }
        }

        // Validation
        public bool IsValid()
        {
            return ExpiryDate > DateTime.Now.AddMonths(-1) && // Grace period
                   !string.IsNullOrWhiteSpace(MaskedCardNumber) &&
                   Amount > 0;
        }
    }
}
