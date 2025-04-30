namespace Bookstore.Checkout.Accessors
{
    public interface IOrderAccessor
    {
        Task<Guid> CreateOrderAsync(Order order);
        Task UpdateOrderStatusAsync(Guid orderId, string status);
        Task<Order> GetOrderWithItemsAsync(Guid orderId);
        Task<bool> ValidateOrderAsync(Guid orderId, string expectedStatus);
    }
}
