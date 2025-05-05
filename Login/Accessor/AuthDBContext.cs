using Microsoft.EntityFrameworkCore;
using BookstoreApp.Login.Accessor.Models;

namespace BookstoreApp.Login.Accessor
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        public DbSet<Users> Users { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<CartItem> CartItem { get; set; }
        public DbSet<Checkout> Checkout { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Sale> Sale { get; set; }
        public DbSet<Book> Book { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<ShippingDetails> ShippingDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>()
                .HasKey(r => new { r.UserId, r.ISBN });
        }
    }
}

