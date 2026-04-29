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
                // Ensure existing tables are updated to decimal (SQL Server Patches)
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
                    END
                    ELSE
                    BEGIN
                        IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[RestockHistory]') AND name = 'QuantityRestocked' AND type_name(system_type_id) = 'int')
                        BEGIN
                            ALTER TABLE [dbo].[RestockHistory] ALTER COLUMN [QuantityRestocked] decimal(18,2) NOT NULL;
                            ALTER TABLE [dbo].[RestockHistory] ALTER COLUMN [StockAfterRestock] decimal(18,2) NOT NULL;
                        END
                    END
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StoreSettings]') AND name = 'AIForecastHorizonHours')
                    BEGIN
                        ALTER TABLE [dbo].[StoreSettings] ADD [AIForecastHorizonHours] int NOT NULL DEFAULT 720;
                        ALTER TABLE [dbo].[StoreSettings] ADD [AISensitivity] float NOT NULL DEFAULT 0.1;
                        ALTER TABLE [dbo].[StoreSettings] ADD [AISeasonalityMode] nvarchar(max) NOT NULL DEFAULT 'multiplicative';
                    END
                    
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StoreSettings]') AND name = 'LastAISync')
                    BEGIN
                        ALTER TABLE [dbo].[StoreSettings] ADD [LastAISync] datetime2 NULL;
                    END";
                
                context.Database.ExecuteSqlRaw(alterTablesSql);
            }
            else
            {
                Console.WriteLine(">>> DB: Initializing PostgreSQL Schema...");
                try 
                {
                    context.Database.EnsureCreated();
                    Console.WriteLine(">>> DB: PostgreSQL Schema Verified/Created.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">>> DB ERROR: Could not create schema: {ex.Message}");
                    throw;
                }
            }
            
            // Call individual seeders
            Console.WriteLine(">>> DB: Seeding Data...");
            SeedProducts(context);
            SeedRestockHistory(context);
            Console.WriteLine(">>> DB: Seeding Complete.");
        }

        public static void SeedProducts(ApplicationDbContext context)
        {
            var seedProducts = new List<Product>
            {
                new Product { Code = "P-001", Name = "Signature Burger", Price = 12.99m, Stock = 100, Category = "Meals", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-002", Name = "Crispy Fries", Price = 4.50m, Stock = 200, Category = "Sides", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-003", Name = "Coca Cola", Price = 2.50m, Stock = 150, Category = "Drinks", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-004", Name = "Chocolate Sundae", Price = 5.99m, Stock = 50, Category = "Desserts", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-005", Name = "Chicken Nuggets", Price = 8.00m, Stock = 80, Category = "Meals", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) },
                new Product { Code = "P-006", Name = "Red Wine", Price = 45.00m, Stock = 20, Category = "Drinks", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-2) },
                new Product { Code = "P-007", Name = "Grilled Salmon", Price = 24.50m, Stock = 15, Category = "Meals", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-1) },
                new Product { Code = "P-008", Name = "Caesar Salad", Price = 9.99m, Stock = 40, Category = "Sides", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-1) }
            };

            foreach (var p in seedProducts)
            {
                if (!context.Products.Any(x => x.Code == p.Code))
                {
                    context.Products.Add(p);
                }
            }
            context.SaveChanges();
        }

        public static void SeedRestockHistory(ApplicationDbContext context)
        {
            if (context.RestockHistory.Any()) return;

            var products = context.Products.ToList();
            var rand = new Random();

            foreach (var product in products)
            {
                for (int i = 0; i < 4; i++)
                {
                    var date = DateTime.Now.AddDays(-rand.Next(1, 30));
                    context.RestockHistory.Add(new RestockHistory
                    {
                        ProductId = product.Id,
                        QuantityRestocked = rand.Next(100, 300),
                        RestockDate = date,
                        StockAfterRestock = 500 - rand.Next(0, 50),
                        Remarks = "Initial Seed Restock"
                    });
                }
            }
            context.SaveChanges();
        }
    }
}
