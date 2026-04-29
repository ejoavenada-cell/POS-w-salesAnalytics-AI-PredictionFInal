using Microsoft.EntityFrameworkCore;
using FoodOrderingSytemAIAnalytics.Models;

namespace FoodOrderingSytemAIAnalytics.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionDetail> TransactionDetails { get; set; } = null!;
        public DbSet<StoreSetting> StoreSettings { get; set; } = null!;
        public DbSet<RestockHistory> RestockHistory { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TransactionDetail Subtotal as computed (Hybrid Syntax)
            if (Database.IsSqlServer())
            {
                modelBuilder.Entity<TransactionDetail>()
                    .Property(td => td.Subtotal)
                    .HasComputedColumnSql("[Quantity] * [Price]");
            }
            else
            {
                modelBuilder.Entity<TransactionDetail>()
                    .Property(td => td.Subtotal)
                    .HasComputedColumnSql("Quantity * Price");
            }

            // Configure Unique Constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();


            // Configure Relationships and Cascades
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.Transaction)
                .WithMany(t => t.TransactionDetails)
                .HasForeignKey(td => td.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionDetail>()
                .HasOne(td => td.Product)
                .WithMany(p => p.TransactionDetails)
                .HasForeignKey(td => td.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data configuration (Optional, handled by script usually)
        }
    }
}
