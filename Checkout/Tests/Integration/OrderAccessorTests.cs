using Microsoft.EntityFrameworkCore;

namespace Bookstore.Checkout.Tests.Accessors
{
    public class OrderAccessorTests : IDisposable
    {
        private readonly CheckoutDbContext _context;
        private readonly OrderAccessor _accessor;

        public OrderAccessorTests()
        {
            var options = new DbContextOptionsBuilder<CheckoutDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new CheckoutDbContext(options);
            _accessor = new OrderAccessor(_context, Mock.Of<ILogger<OrderAccessor>>());
        }

        [Fact]
        public async Task CreateOrderAsync_SavesOrderWithItems()
        {
            // Arrange
            var order = new Order
            {
                UserId = Guid.NewGuid(),
                Items = new List<OrderItem>
                {
                    new OrderItem { BookId = "9788414295080", Quantity = 2 }
                }
            };

            // Act
            var orderId = await _accessor.CreateOrderAsync(order);

            // Assert
            var savedOrder = await _context.Orders
                .Include(o => o.Items)
                .FirstAsync();
            
            Assert.Single(savedOrder.Items);
            Assert.Equal("9788414295080", savedOrder.Items[0].BookId);
        }

        public void Dispose() => _context.Dispose();
    }
}
