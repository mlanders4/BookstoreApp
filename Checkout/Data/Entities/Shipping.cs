using System.ComponentModel.DataAnnotations.Schema;

namespace Bookstore.Checkout.Data.Entities
{
    [Table("ShippingDetails")] // Maps to your actual table name
    public class Shipping
    {
        [Column("ship_id")]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("street")]
        public string StreetAddress { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("zip")]
        public string PostalCode { get; set; }

        [Column("country")]
        public string Country { get; set; }
    }
}
