using System.ComponentModel.DataAnnotations;
namespace BookstoreApp.Login.Accessor
{
    public class ShippingDetails
    {
        [Key]
        public int ShipId { get; set; }
        public int OrderId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
    }
}