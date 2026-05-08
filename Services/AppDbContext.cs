using BIsm2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BIsm2.Services
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductMedia> ProductMedias { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Checkout> Checkouts { get; set; }
        public DbSet<CheckoutItem> CheckoutItems { get; set; }

        //     public DbSet<CartCheckoutViewModel> CartCheckoutViewModels { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product → Media
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Media)
                .WithOne(m => m.Product)
                .HasForeignKey(m => m.ProductId);

            // Product → Comments
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Product)
                .HasForeignKey(c => c.ProductId);

            // Cart → Items
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId);

            // CartItem → Product
            modelBuilder.Entity<CartItem>()
                .HasOne(i => i.Product)
                .WithMany() // Product doesn’t need back-reference
                .HasForeignKey(i => i.ProductId);

            // CartItem → Cart
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId);

            // Order → Product
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId);
            
            modelBuilder.Entity<Order>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<Order>()
                .Property(o => o.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<ProductMedia>()
             .Property(pm => pm.FileData)
             .HasColumnType("bytea"); // Neon/Postgres binary type


            modelBuilder.Entity<CheckoutItem>()
                .HasOne(ci => ci.Checkout)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CheckoutId);

            modelBuilder.Entity<CheckoutItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);



            // Ignore Identity types not supported by Npgsql
            modelBuilder.Ignore<IdentityUserPasskey<string>>();
            modelBuilder.Ignore<IdentityPasskeyData>();
        }
    }
}

 