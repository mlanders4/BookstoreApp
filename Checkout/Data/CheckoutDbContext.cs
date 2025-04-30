using Bookstore.Checkout.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Checkout.Data
{
    public class CheckoutDbContext : DbContext
    {
        public CheckoutDbContext(DbContextOptions<CheckoutDbContext> options) 
            : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<ShippingDetail> ShippingDetails { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Book Entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.ToTable("Book");
                entity.HasKey(b => b.ISBN);
                entity.Property(b => b.ISBN).HasColumnName("isbn");
                entity.HasOne(b => b.Sale)
                      .WithMany()
                      .HasForeignKey(b => b.SaleId);
            });

            // Order Entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.Property(o => o.Id).HasColumnName("order_id");
                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId);
                entity.HasOne(o => o.Cart)
                      .WithMany()
                      .HasForeignKey(o => o.CartId);
                entity.HasOne(o => o.Checkout)
                      .WithMany()
                      .HasForeignKey(o => o.CheckoutId);
            });

            // Configure GUID handling for SQL Server
            modelBuilder.Entity<Order>()
                .Property(o => o.Id)
                .HasDefaultValueSql("NEWID()");

            // Other entity configurations...
            modelBuilder.Entity<ShippingDetail>().ToTable("ShippingDetails");
            modelBuilder.Entity<CartItem>().ToTable("CartItem");
            modelBuilder.Entity<Checkout>().ToTable("Checkout");
        }
    }
}
