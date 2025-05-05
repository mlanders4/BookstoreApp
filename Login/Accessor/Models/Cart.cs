namespace BookstoreApp.Login.Accessor
{
    public class Cart
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}