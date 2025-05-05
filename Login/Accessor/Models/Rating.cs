namespace BookstoreApp.Login.Accessor
{
    public class Rating
    {
        public int UserId { get; set; }
        public string ISBN { get; set; }
        public int RatingValue { get; set; }
        public string Review { get; set; }
    }
}