namespace Bookstore.Checkout.Data.Entities
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string MaskedCardNumber { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        
        // Navigation
        public Order Order { get; set; }
    }
}
