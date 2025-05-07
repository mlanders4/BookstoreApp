using Microsoft.EntityFrameworkCore;

namespace Bookstore.Checkout.Data
{
    public class CheckoutDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Shipping> ShippingDetails { get; set; }

        public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Only configure what you need
            modelBuilder.Entity<Order>(entity => 
            {
                entity.ToTable("Orders");
                entity.HasKey(o => o.Id);
            });

            modelBuilder.Entity<Payment>(entity => 
            {
                entity.ToTable("Checkout");
                entity.HasKey(p => p.Id);
            });

            modelBuilder.Entity<Shipping>(entity =>
            {
                entity.ToTable("ShippingDetails");
                entity.HasKey(s => s.Id);
            });
        }
    }
}
