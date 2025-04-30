namespace Catalog.Accessor
{
    public class Book
    {
        private string isbn;
        private string name;
        private string author;
        private string description;
        private string productImage;
        private string dimension;
        private decimal weight;
        private decimal price;
        private string category;
        private int saleId;
        private string saleStatus;

        public string ISBN
        {
            get { return isbn; }
            set { isbn = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Author
        {
            get { return author; }
            set { author = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        public string ProductImage
        {
            get { return productImage; }
            set { productImage = value; }
        }

        public string Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }

        public decimal Weight
        {
            get { return weight; }
            set { weight = value; }
        }

        public decimal Price
        {
            get { return price; }
            set { price = value; }
        }

        public string Category
        {
            get { return category; }
            set { category = value; }
        }

        public int SaleId
        {
            get { return saleId; }
            set { saleId = value; }
        }

        public string SaleStatus
        {
            get { return saleStatus; }
            set { saleStatus = value; }
        }
    }
} 
