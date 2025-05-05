using System.ComponentModel.DataAnnotations;

namespace BookstoreApp.Login.Accessor.Models
{
    public class Book
    {
        [Key] 
        public int BookId { get; set; }

        public string ISBN { get; set; }
        public string Name { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string ProductImage { get; set; }
        public string Dimension { get; set; }
        public string Category { get; set; }
        public string SaleStatus { get; set; }
    }
}
