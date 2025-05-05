using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
namespace Catalog.Accessor

{
    public class CatalogAccessor
    {
        private string connectionString;

        // SQL queries
        private const string SelectBooks = "SELECT * FROM Book";
        private const string SelectBookByIsbn = "SELECT * FROM Book WHERE isbn = @ISBN";
        private const string SelectBookBySearch = @"
    SELECT * FROM Book 
    WHERE 
        name LIKE @SearchTerm OR 
        author LIKE @SearchTerm OR 
        category LIKE @SearchTerm OR
        isbn LIKE @SearchTerm";
        private const string SelectCategories = "SELECT DISTINCT category FROM Book";
        private const string InsertBook = @"
            INSERT INTO Book (
                isbn, name, author, description, product_image, dimension, 
                weight, price, category, sale_id, sale_status
            ) VALUES (
                @ISBN, @Name, @Author, @Description, @ProductImage, @Dimension,
                @Weight, @Price, @Category, @SaleId, @SaleStatus
            )";

        private const string UpdateBookQuery = @"
            UPDATE Book SET 
                name = @Name, author = @Author, description = @Description,
                product_image = @ProductImage, dimension = @Dimension,
                weight = @Weight, price = @Price, category = @Category,
                sale_id = @SaleId, sale_status = @SaleStatus
            WHERE isbn = @ISBN";
        private const string DeleteBookQuery = "DELETE FROM Book WHERE isbn = @ISBN";
        private const string SelectBooksBySale = "SELECT * FROM Book WHERE sale_id = @SaleId";
        private const string SelectBooksByCategory = "SELECT * FROM Book WHERE category = @Category";

        public CatalogAccessor(string connectionString)
        {
            this.connectionString = connectionString;
        }

        private Book CreateBookFromReader(SqlDataReader reader)
        {
            return new Book
            {
                ISBN = reader["isbn"].ToString(),
                Name = reader["name"].ToString(),
                Author = reader["author"].ToString(),
                Description = reader["description"].ToString(),
                ProductImage = reader["product_image"].ToString(),
                Dimension = reader["dimension"].ToString(),
                Weight = Convert.ToDecimal(reader["weight"]),
                Price = Convert.ToDecimal(reader["price"]),
                Category = reader["category"].ToString(),
                SaleId = Convert.ToInt32(reader["sale_id"]),
                SaleStatus = reader["sale_status"].ToString()
            };
        }

        private void AddBookParameters(SqlCommand command, Book book)
        {
            command.Parameters.AddWithValue("@ISBN", book.ISBN);
            command.Parameters.AddWithValue("@Name", book.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Author", book.Author ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", book.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@ProductImage", book.ProductImage ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Dimension", book.Dimension ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Weight", book.Weight);
            command.Parameters.AddWithValue("@Price", book.Price);
            command.Parameters.AddWithValue("@Category", book.Category ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@SaleId", book.SaleId);
            command.Parameters.AddWithValue("@SaleStatus", book.SaleStatus ?? "regular");
        }

        public List<Book> GetAllBooks()
        {
            var books = new List<Book>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SelectBooks, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        books.Add(CreateBookFromReader(reader));
                    }
                }
            }
            return books;
        }

        public Book GetBookByIsbn(string isbn)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SelectBookByIsbn, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", isbn);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return CreateBookFromReader(reader);
                        }
                    }
                }
            }
            return null;
        }

        public List<Book> SearchBooks(string searchTerm)
        {
            var books = new List<Book>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SelectBookBySearch, connection))
                {
                    command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(CreateBookFromReader(reader));
                        }
                    }
                }
            }
            return books;
        }

        public List<string> GetCategories()
        {
            var categories = new List<string>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SelectCategories, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(reader["category"].ToString());
                    }
                }
            }
            return categories;
        }

        public bool CreateBook(Book book)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(InsertBook, connection))
                {
                    AddBookParameters(command, book);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateBook(Book book)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(UpdateBookQuery, connection))
                {
                    AddBookParameters(command, book);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteBook(string isbn)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(DeleteBookQuery, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", isbn);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<Book> GetBooksByCategory(string category)
        {
            var books = new List<Book>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SelectBooksByCategory, connection))
                {
                    command.Parameters.AddWithValue("@Category", category);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(CreateBookFromReader(reader));
                        }
                    }
                }
            }
            return books;
        }

        public List<Book> GetBooksBySale(int saleId)
        {
            var books = new List<Book>();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(SelectBooksBySale, connection))
                {
                    command.Parameters.AddWithValue("@SaleId", saleId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            books.Add(CreateBookFromReader(reader));
                        }
                    }
                }
            }
            return books;
        }
    }
}