namespace Bookstore.Checkout.Data.Entities
{
    public class Order
    {
        [Column("order_id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("cart_id")] 
        public int CartId { get; set; }

        [Column("checkout_id")]
        public int CheckoutId { get; set; }

        [Column("date")]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow.Date;

        [Column("status")]
        public string Status { get; set; } = "pending"; // pending/processing/shipped/cancelled/completed

        // Navigation properties
        public Payment Payment { get; set; }
        public ShippingDetail ShippingDetail { get; set; }
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string BookId { get; set; }  // ISBN
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
