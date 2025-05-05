namespace BookstoreApp.Login.Accessor
{
    public class Checkout
    {
        public int CheckoutId { get; set; }
        public string CreditCardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public decimal Amount { get; set; }
    }
}