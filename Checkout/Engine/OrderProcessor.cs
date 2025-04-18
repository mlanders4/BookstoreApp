public interface IOrderProcessor
{
    Order CreateOrder(string userId, List<CartItem> items, ShippingOption shipping, string paymentReference);
}

public class OrderProcessor : IOrderProcessor
{
    public Order CreateOrder(string userId, List<CartItem> items, ShippingOption shipping, string paymentReference)
    {
        if (items == null || !items.Any())
            throw new ArgumentException("Cart cannot be empty");

        var order = new Order
        {
            OrderId = GenerateOrderId(),
            UserId = userId,
            OrderDate = DateTime.Now,
            Status = OrderStatus.Processing,
            ShippingOption = shipping,
            PaymentReference = paymentReference,
            Items = items.Select(ConvertToOrderItem).ToList()
        };

        order.Subtotal = order.Items.Sum(i => i.TotalPrice);
        order.Total = order.Subtotal + shipping.Cost;

        return order;
    }

    private string GenerateOrderId() => $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}";

    private OrderItem ConvertToOrderItem(CartItem cartItem) => new()
    {
        BookId = cartItem.BookId,
        Title = cartItem.Title,
        Quantity = cartItem.Quantity,
        UnitPrice = cartItem.Price,
        TotalPrice = cartItem.Price * cartItem.Quantity
    };
}
