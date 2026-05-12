using GameHubStore.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameHubStore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<GameKey> GameKeys { get; set; }
        public DbSet<GamePlatform> GamePlatforms { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Game price precision
            builder.Entity<Game>()
                .Property(g => g.Price)
                .HasPrecision(18, 2);

            builder.Entity<Game>()
                .Property(g => g.DiscountPrice)
                .HasPrecision(18, 2);

            // CartItem price precision
            builder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            // Order total precision
            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            // OrderItem price precision
            builder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);

            // Payment amount precision
            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            // Category -> Games
            builder.Entity<Game>()
                .HasOne(g => g.Category)
                .WithMany(c => c.Games)
                .HasForeignKey(g => g.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //// Platform -> Games
            //builder.Entity<Game>()
            //    .HasOne(g => g.Platform)
            //    .WithMany(p => p.Games)
            //    .HasForeignKey(g => g.PlatformId)
            //    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GamePlatform>()
            .HasKey(gp => new { gp.GameId, gp.PlatformId });

            builder.Entity<GamePlatform>()
                .HasOne(gp => gp.Game)
                .WithMany(g => g.GamePlatforms)
                .HasForeignKey(gp => gp.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<GamePlatform>()
                .HasOne(gp => gp.Platform)
                .WithMany(p => p.GamePlatforms)
                .HasForeignKey(gp => gp.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Orders
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> Payment one-to-one
            builder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> CartItems
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Game -> CartItems
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Game)
                .WithMany(g => g.CartItems)
                .HasForeignKey(ci => ci.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order -> OrderItems
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Game -> OrderItems
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Game)
                .WithMany(g => g.OrderItems)
                .HasForeignKey(oi => oi.GameId)
                .OnDelete(DeleteBehavior.Restrict);

            // Game -> GameKeys
            builder.Entity<GameKey>()
                .HasOne(gk => gk.Game)
                .WithMany(g => g.GameKeys)
                .HasForeignKey(gk => gk.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Purchased GameKeys
            builder.Entity<GameKey>()
                .HasOne(gk => gk.SoldToUser)
                .WithMany(u => u.PurchasedKeys)
                .HasForeignKey(gk => gk.SoldToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // One cart per user
            builder.Entity<Cart>()
                .HasIndex(c => c.UserId)
                .IsUnique();

            // Same game should not appear twice in same cart
            builder.Entity<CartItem>()
                .HasIndex(ci => new { ci.CartId, ci.GameId })
                .IsUnique();

            // Game key should be unique
            builder.Entity<GameKey>()
                .HasIndex(gk => gk.KeyCode)
                .IsUnique();
        }
    }
}