using Bookstore.Checkout.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Checkout.Data
{
    public class CheckoutDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ShippingDetail> ShippingDetails { get; set; }

        public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map Order to existing table
            modelBuilder.Entity<Order>(entity => 
            {
                entity.ToTable("Orders");
                entity.Property(o => o.Id)
                    .HasColumnName("order_id")
                    .HasConversion(
                        v => v.ToString(),  // Guid to string
                        v => Guid.Parse(v));
            });

            // Treat other tables as read-only
            modelBuilder.Entity<Payment>(entity => 
            {
                entity.ToTable("Checkout");
                entity.HasNoKey();
            });

            // Configure ShippingDetails if needed
            modelBuilder.Entity<ShippingDetail>(entity =>
            {
                entity.ToTable("ShippingDetails");
                entity.HasNoKey();
            });
        }
    }
}
