using Bookstore.Checkout.Data.Entities;
using System.Threading.Tasks;

namespace Bookstore.Checkout.Accessors
{
    public interface IOrderAccessor
    {
        /// <summary>
        /// Creates a new order and returns its ID
        /// </summary>
        Task<int> CreateOrderAsync(Order order);

        /// <summary>
        /// Updates order status with validation
        /// </summary>
        Task UpdateOrderStatusAsync(int orderId, string status);

        /// <summary>
        /// Retrieves order by ID including related items
        /// </summary>
        Task<Order> GetOrderWithItemsAsync(int orderId);

        /// <summary>
        /// Validates order existence and status
        /// </summary>
        Task<bool> ValidateOrderAsync(int orderId, string expectedStatus);
    }
}
