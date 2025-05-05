namespace BookstoreApp.Login.Accessor
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public string ISBN { get; set; }
        public int Quantity { get; set; }
    }
}