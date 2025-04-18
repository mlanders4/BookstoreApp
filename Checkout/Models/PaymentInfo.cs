namespace Bookstore.Checkout.Models;

public record PaymentInfo(
    string CardNumber,              // 13-19 digits (no real processing)
    string CardholderName,          // Name on card
    DateTime ExpiryDate,            // Future date
    string Cvv                      // 3-4 digits
)
{
    public bool IsValid() 
        => CardNumber.Length is >= 13 and <= 19 
           && ExpiryDate > DateTime.Now 
           && Cvv.Length is 3 or 4;
}
