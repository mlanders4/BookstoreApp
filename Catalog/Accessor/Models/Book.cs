using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BookstoreApp.Catalog.Accessor.Models
{
    [Table("Book")]
    public class Book
    {
        [Key]
        public string Isbn { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string Product_Image { get; set; }
        public string Dimension { get; set; }
        public decimal Weight { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Sale_Status { get; set; }
    }
}