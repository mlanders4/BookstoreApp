namespace Bookstore.Checkout.Contracts
{
    // Simplified models for frontend
    public record ClientCartItem(
        string BookId, 
        string Title, 
        decimal Price, 
        int Quantity);

    public record ClientAddress(
        string Street,
        string City,
        string State,
        string ZipCode,
        string Country);

    public record ClientPaymentMethod(
        string CardType,
        string LastFourDigits,
        string ExpiryDate);
}
