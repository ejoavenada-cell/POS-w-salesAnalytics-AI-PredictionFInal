using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace FoodOrderingSytemAIAnalytics.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Database.IsSqlServer())
            {
                // SQL Server specific patches
                string alterTablesSql = @"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Products]') AND name = 'Stock' AND (type_name(system_type_id) = 'int' OR type_name(system_type_id) = 'numeric'))
                    BEGIN
                        ALTER TABLE [dbo].[Products] ALTER COLUMN [Stock] decimal(18,2) NULL;
                    END

                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransactionDetails]') AND name = 'Quantity' AND type_name(system_type_id) = 'int')
                    BEGIN
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[TransactionDetails]') AND name = 'Subtotal' AND is_computed = 1)
                        BEGIN
                            ALTER TABLE [dbo].[TransactionDetails] DROP COLUMN [Subtotal];
                        END
                        
                        ALTER TABLE [dbo].[TransactionDetails] ALTER COLUMN [Quantity] decimal(18,2) NOT NULL;
                        
                        ALTER TABLE [dbo].[TransactionDetails] ADD [Subtotal] AS ([Quantity] * [Price]);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RestockHistory]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[RestockHistory] (
                            [Id] int IDENTITY(1,1) NOT NULL,
                            [ProductId] int NOT NULL,
                            [QuantityRestocked] decimal(18,2) NOT NULL,
                            [RestockDate] datetime2 NOT NULL,
                            [Remarks] nvarchar(max) NULL,
                            [StockAfterRestock] decimal(18,2) NOT NULL,
                            CONSTRAINT [PK_RestockHistory] PRIMARY KEY ([Id]),
                            CONSTRAINT [FK_RestockHistory_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE
                        );
                        CREATE INDEX [IX_RestockHistory_ProductId] ON [dbo].[RestockHistory] ([ProductId]);
                    END";
                
                try { context.Database.ExecuteSqlRaw(alterTablesSql); } catch { }
            }
            else
            {
                Console.WriteLine(">>> DB: Initializing PostgreSQL Schema...");
                try 
                {
                    // For Postgres, EnsureCreated is the most reliable way to build a fresh schema
                    context.Database.EnsureCreated();
                    Console.WriteLine(">>> DB: PostgreSQL Schema Verified.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">>> DB ERROR: {ex.Message}");
                }
            }
            
            // Call individual seeders
            Console.WriteLine(">>> DB: Seeding Data...");
            SeedStoreSettings(context);
            SeedProducts(context);
            SeedRestockHistory(context);
            Console.WriteLine(">>> DB: Seeding Complete.");
        }

        public static void SeedStoreSettings(ApplicationDbContext context)
        {
            if (!context.StoreSettings.Any())
            {
                context.StoreSettings.Add(new StoreSettings
                {
                    StoreName = "Tasty Station",
                    CurrencySymbol = "$",
                    TaxRate = 0.08m,
                    AIForecastHorizonHours = 720,
                    AISensitivity = 0.1,
                    AISeasonalityMode = "multiplicative"
                });
                context.SaveChanges();
            }
        }

        public static void SeedProducts(ApplicationDbContext context)
        {
            if (context.Products.Any()) return;
            var seedProducts = new List<Product>
            {
                new Product { Code = "P-001", Name = "Signature Burger", Price = 12.99m, Stock = 100, Category = "Meals", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-002", Name = "Crispy Fries", Price = 4.50m, Stock = 200, Category = "Sides", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-003", Name = "Coca Cola", Price = 2.50m, Stock = 150, Category = "Drinks", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-004", Name = "Chocolate Sundae", Price = 5.99m, Stock = 50, Category = "Desserts", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-005", Name = "Chicken Nuggets", Price = 8.00m, Stock = 80, Category = "Meals", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) }
            };
            context.Products.AddRange(seedProducts);
            context.SaveChanges();
        }

        public static void SeedRestockHistory(ApplicationDbContext context)
        {
            if (context.RestockHistory.Any()) return;

            var products = context.Products.ToList();
            var rand = new Random();

            foreach (var product in products)
            {
                for (int i = 0; i < 2; i++)
                {
                    context.RestockHistory.Add(new RestockHistory
                    {
                        ProductId = product.Id,
                        QuantityRestocked = rand.Next(100, 300),
                        RestockDate = DateTime.Now.AddDays(-rand.Next(1, 30)),
                        StockAfterRestock = 500 - rand.Next(0, 50),
                        Remarks = "Initial Seed Restock"
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
