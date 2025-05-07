using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Data.Entities
{
    [Table("Checkout")] // Matches your table name
    public class Payment
    {
        [Column("checkout_id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("credit_card_number")]
        public string MaskedCardNumber { get; set; }

        [Column("expiry_date")]
        public DateTime ExpiryDate { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("status")] // Matches your "payment_status" column if different
        public string Status { get; set; } = "pending";
    }
}
