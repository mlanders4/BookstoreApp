namespace Bookstore.Checkout.Data.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        
        // Navigation properties
        public Payment Payment { get; set; }
        public Shipping Shipping { get; set; }
    }
}
