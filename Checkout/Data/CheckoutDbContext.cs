using Microsoft.EntityFrameworkCore;
using Bookstore.Checkout.Data.Entities;

namespace Bookstore.Checkout.Data
{
    public class CheckoutDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipping> Shippings { get; set; }

        public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure composite keys or relationships here
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne()
                .HasForeignKey(i => i.OrderId);
        }
    }
}
