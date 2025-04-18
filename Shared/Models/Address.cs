namespace Bookstore.Shared.Models;

public record Address(
    string Street,
    string City,
    string PostalCode,
    string Country
)
{
    public bool IsValid() => 
        !string.IsNullOrWhiteSpace(Street) &&
        !string.IsNullOrWhiteSpace(PostalCode);
}
