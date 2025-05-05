using System.ComponentModel.DataAnnotations;
namespace BookstoreApp.Login.Accessor
{
    public class Orders
    {
        [Key]
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int CartId { get; set; }
        public int CheckoutId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}