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
            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.Property(o => o.Id).HasColumnName("order_id");
                entity.HasOne(o => o.Payment)
                      .WithOne()
                      .HasForeignKey<Payment>(p => p.OrderId);
                entity.HasOne(o => o.ShippingDetail)
                      .WithOne()
                      .HasForeignKey<ShippingDetail>(s => s.OrderId);
            });

            // SQL Server GUID handling
            modelBuilder.Entity<Order>()
                .Property(o => o.Id)
                .HasDefaultValueSql("NEWID()");
        }
    }
}
