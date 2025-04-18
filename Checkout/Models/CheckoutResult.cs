namespace Bookstore.Checkout.Models;

public record CheckoutResult(
    bool IsSuccess,                 // True if checkout succeeded
    Order? Order = null,            // Created order details
    string? Error = null            // Failure reason (if any)
)
{
    public static CheckoutResult Success(Order order) 
        => new(true, order);
    
    public static CheckoutResult Failure(string error) 
        => new(false, null, error);
}
