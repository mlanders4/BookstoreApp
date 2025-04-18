namespace Bookstore.Shared.Models;

public record CartItem(
    string BookId,
    string Title,
    decimal Price,
    int Quantity
);
