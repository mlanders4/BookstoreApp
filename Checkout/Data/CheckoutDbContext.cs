using Bookstore.Checkout.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Checkout.Data
{
    public class CheckoutDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipping> Shippings { get; set; }

        public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.Id).ValueGeneratedOnAdd();
                entity.HasMany(o => o.Items)
                      .WithOne()
                      .HasForeignKey(i => i.OrderId);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id).ValueGeneratedOnAdd();
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Id).ValueGeneratedOnAdd();
                entity.HasOne(p => p.Order)
                      .WithOne(o => o.Payment)
                      .HasForeignKey<Payment>(p => p.OrderId);
            });

            // Shipping configuration
            modelBuilder.Entity<Shipping>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Id).ValueGeneratedOnAdd();
                entity.HasOne(s => s.Order)
                      .WithOne(o => o.Shipping)
                      .HasForeignKey<Shipping>(s => s.OrderId);
            });
        }
    }
}
