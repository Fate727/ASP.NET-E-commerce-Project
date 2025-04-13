using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Techhive.Models;
using Techhive.OrderModels;
using Techhive.ProductModels;
using Techhive.TrendingModel;

namespace Techhive.Data
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }
        public DbSet<Trending> Trendings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product ↔ Brand
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brands)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.B_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart ↔ User
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart ↔ Product
            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Product)
                .WithMany()
                .HasForeignKey(c => c.P_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 Recommendation ↔ User
            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 Recommendation ↔ Product
            modelBuilder.Entity<Recommendation>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.P_Id)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔥 Trending ↔ Product
            modelBuilder.Entity<Trending>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.P_Id)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
