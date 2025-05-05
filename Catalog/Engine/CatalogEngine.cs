using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.Accessor;

namespace Catalog.Engine
{
    public class CatalogEngine
    {
        private CatalogAccessor catalogAccessor;

        public CatalogEngine(string connectionString)
        {
            this.catalogAccessor = new CatalogAccessor(connectionString);
        }

        public List<Book> GetAllBooks()
        {
            return catalogAccessor.GetAllBooks();
        }

        public Book GetBookByIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN is required");
                
            return catalogAccessor.GetBookByIsbn(isbn);
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Book>();
                
            return catalogAccessor.SearchBooks(searchTerm);
        }

        public List<string> GetCategories()
        {
            return catalogAccessor.GetCategories();
        }

        public bool CreateBook(Book book)
        {
            ValidateBook(book);
            
            if (book.Price <= 0)
                book.Price = 9.99m;
                
            if (string.IsNullOrWhiteSpace(book.SaleStatus))
                book.SaleStatus = "regular";
                
            return catalogAccessor.CreateBook(book);
        }

        public bool UpdateBook(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("ISBN is required");
                
            ValidateBook(book);
            
            return catalogAccessor.UpdateBook(book);
        }

        public bool DeleteBook(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN is required");
                
            return catalogAccessor.DeleteBook(isbn);
        }

        public bool IsBookAvailable(string isbn)
        {
            var book = GetBookByIsbn(isbn);
            
            if (book == null)
                return false;
                
            return !book.SaleStatus.Equals("preorder", StringComparison.OrdinalIgnoreCase);
        }

        public List<Book> GetBooksByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return new List<Book>();
                
            return catalogAccessor.GetBooksByCategory(category);
        }

        public List<Book> GetBooksBySale(int saleId)
        {
            if (saleId <= 0)
                throw new ArgumentException("Sale ID must be positive");
                
            return catalogAccessor.GetBooksBySale(saleId);
        }

        private void ValidateBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));
                
            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("ISBN is required");
                
            if (string.IsNullOrWhiteSpace(book.Name))
                throw new ArgumentException("Book name is required");
                
            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Book author is required");
                
            if (book.ISBN.Length > 28)
                throw new ArgumentException("ISBN must be 28 characters or less");
                
            if (book.Price < 0)
                throw new ArgumentException("Book price cannot be negative");
                
            if (!string.IsNullOrWhiteSpace(book.SaleStatus) && 
                !book.SaleStatus.Equals("regular", StringComparison.OrdinalIgnoreCase) && 
                !book.SaleStatus.Equals("on_sale", StringComparison.OrdinalIgnoreCase) && 
                !book.SaleStatus.Equals("preorder", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Sale status must be 'regular', 'on_sale', or 'preorder'");
            }
        }
        
        public decimal CalculatePrice(Book book, bool applyDiscount)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));
                
            decimal price = book.Price;
            
            if (applyDiscount && book.SaleStatus.Equals("on_sale", StringComparison.OrdinalIgnoreCase))
            {
                price *= 0.9m; 
            }
            
            return Math.Round(price, 2);
        }
    }
}
=======
using System;
using System.Collections.Generic;
using System.Linq;
using Catalog.Accessor;

namespace Catalog.Engine
{
    public class CatalogEngine
    {
        private readonly CatalogAccessor catalogAccessor;

        public CatalogEngine(CatalogAccessor catalogAccessor)
        {
            this.catalogAccessor = catalogAccessor;
        }

        public List<Book> GetAllBooks()
        {
            return catalogAccessor.GetAllBooks();
        }

        public Book GetBookByIsbn(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN is required");

            return catalogAccessor.GetBookByIsbn(isbn);
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<Book>();

            return catalogAccessor.SearchBooks(searchTerm);
        }

        public List<string> GetCategories()
        {
            return catalogAccessor.GetCategories();
        }

        public bool CreateBook(Book book)
        {
            ValidateBook(book);

            if (book.Price <= 0)
                book.Price = 9.99m;

            if (string.IsNullOrWhiteSpace(book.SaleStatus))
                book.SaleStatus = "regular";

            return catalogAccessor.CreateBook(book);
        }

        public bool UpdateBook(Book book)
        {
            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("ISBN is required");

            ValidateBook(book);

            return catalogAccessor.UpdateBook(book);
        }

        public bool DeleteBook(string isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn))
                throw new ArgumentException("ISBN is required");

            return catalogAccessor.DeleteBook(isbn);
        }

        public bool IsBookAvailable(string isbn)
        {
            var book = GetBookByIsbn(isbn);

            if (book == null)
                return false;

            return !book.SaleStatus.Equals("preorder", StringComparison.OrdinalIgnoreCase);
        }

        public List<Book> GetBooksByCategory(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return new List<Book>();

            return catalogAccessor.GetBooksByCategory(category);
        }

        public List<Book> GetBooksBySale(int saleId)
        {
            if (saleId <= 0)
                throw new ArgumentException("Sale ID must be positive");

            return catalogAccessor.GetBooksBySale(saleId);
        }

        private void ValidateBook(Book book)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            if (string.IsNullOrWhiteSpace(book.ISBN))
                throw new ArgumentException("ISBN is required");

            if (string.IsNullOrWhiteSpace(book.Name))
                throw new ArgumentException("Book name is required");

            if (string.IsNullOrWhiteSpace(book.Author))
                throw new ArgumentException("Book author is required");

            if (book.ISBN.Length > 28)
                throw new ArgumentException("ISBN must be 28 characters or less");

            if (book.Price < 0)
                throw new ArgumentException("Book price cannot be negative");

            if (!string.IsNullOrWhiteSpace(book.SaleStatus) &&
                !book.SaleStatus.Equals("regular", StringComparison.OrdinalIgnoreCase) &&
                !book.SaleStatus.Equals("on_sale", StringComparison.OrdinalIgnoreCase) &&
                !book.SaleStatus.Equals("preorder", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Sale status must be 'regular', 'on_sale', or 'preorder'");
            }
        }

        public decimal CalculatePrice(Book book, bool applyDiscount)
        {
            if (book == null)
                throw new ArgumentNullException(nameof(book));

            decimal price = book.Price;

            if (applyDiscount && book.SaleStatus.Equals("on_sale", StringComparison.OrdinalIgnoreCase))
            {
                price *= 0.9m;
            }

            return Math.Round(price, 2);
        }
    }
}
