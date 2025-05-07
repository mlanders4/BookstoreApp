using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Data.Entities
{
    [Table("Checkout")]
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

        [Column("payment_status")]
        public string Status { get; set; } = "pending";

        public virtual Order Order { get; set; }
    }
}
