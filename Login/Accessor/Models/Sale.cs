namespace BookstoreApp.Login.Accessor
{
    public class Sale
    {
        public int SaleId { get; set; }
        public string Code { get; set; }
        public decimal Discount { get; set; }
        public DateTime SDate { get; set; }
        public DateTime EDate { get; set; }
    }
}