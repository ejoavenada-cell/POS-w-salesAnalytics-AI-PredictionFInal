using FoodOrderingSytemAIAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FoodOrderingSytemAIAnalytics.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            // Safety check for missing Categories table (EnsureCreated doesn't update existing schemas)
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Categories]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Categories] (
                        [Id] INT IDENTITY(1,1) PRIMARY KEY,
                        [Name] NVARCHAR(50) NOT NULL,
                        [Icon] NVARCHAR(50) NOT NULL,
                        [IsActive] BIT NOT NULL DEFAULT 1
                    );
                END
            ");

            // Safety check for missing User ImageUrl column
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'ImageUrl')
                BEGIN
                    ALTER TABLE [dbo].[Users] ADD [ImageUrl] NVARCHAR(500) NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Transactions]') AND name = 'IsWeekend')
                BEGIN
                    ALTER TABLE [dbo].[Transactions] ADD [IsWeekend] BIT NOT NULL DEFAULT 0;
                END
            ");

            // Safety check for missing StoreSettings table
            context.Database.ExecuteSqlRaw(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StoreSettings]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[StoreSettings] (
                        [Id] INT IDENTITY(1,1) PRIMARY KEY,
                        [StoreName] NVARCHAR(100) NOT NULL,
                        [LogoUrl] NVARCHAR(500) NULL,
                        [CurrencySymbol] NVARCHAR(10) NOT NULL DEFAULT '₱'
                    );
                END
            ");

            // Seed Settings
            if (!context.StoreSettings.Any())
            {
                context.StoreSettings.Add(new StoreSetting { StoreName = "Tasty Station", LogoUrl = "/images/logo.png" });
                context.SaveChanges();
            }

            // Seed Categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Burger", Icon = "fas fa-hamburger" },
                    new Category { Name = "Pizza", Icon = "fas fa-pizza-slice" },
                    new Category { Name = "Ice Cream", Icon = "fas fa-ice-cream" },
                    new Category { Name = "Juice", Icon = "fas fa-glass-whiskey" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var products = new List<Product>
            {
                // Burger Category
                new Product { Name = "Classic Chicken Burger", Code = "BRG-001", Price = 5.47m, Category = "Burger", Stock = 100, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-60), ImageUrl = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?auto=format&fit=crop&w=400&h=400" },
                new Product { Name = "Double Cheese Burger", Code = "BRG-002", Price = 6.10m, Category = "Burger", Stock = 100, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-30), ImageUrl = "https://images.unsplash.com/photo-1550547660-d9450f859349?auto=format&fit=crop&w=400&h=400" },
                new Product { Name = "Spicy Zinger Burger", Code = "BRG-003", Price = 5.99m, Category = "Burger", Stock = 80, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-5), ImageUrl = "https://images.unsplash.com/photo-1594212699903-ec8a3eca50f5?auto=format&fit=crop&w=400&h=400" },

                // Pizza Category
                new Product { Name = "Margherita Pizza", Code = "PZ-001", Price = 7.00m, Category = "Pizza", Stock = 50, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-45), ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbad80ad50?auto=format&fit=crop&w=400&h=400" },
                new Product { Name = "Chicken Mushroom Pizza", Code = "PZ-002", Price = 8.50m, Category = "Pizza", Stock = 40, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-15), ImageUrl = "https://images.unsplash.com/photo-1513104890138-7c749659a591?auto=format&fit=crop&w=400&h=400" },
                new Product { Name = "Pepperoni Feast", Code = "PZ-003", Price = 9.00m, Category = "Pizza", Stock = 30, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-2), ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?auto=format&fit=crop&w=400&h=400" },

                // Ice Cream / Desserts
                new Product { Name = "Triple Scope Vanilla", Code = "IC-001", Price = 2.47m, Category = "Ice Cream", Stock = 200, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-10), ImageUrl = "https://images.unsplash.com/photo-1501443762994-82bd5dace89a?auto=format&fit=crop&w=400&h=400" },
                new Product { Name = "Chocolate Lava Cake", Code = "IC-002", Price = 4.50m, Category = "Ice Cream", Stock = 50, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-20), ImageUrl = "https://images.unsplash.com/photo-1624353365286-3f8d62ffff51?auto=format&fit=crop&w=400&h=400" },

                // Drinks / Juice
                new Product { Name = "Fresh Orange Juice", Code = "DRK-001", Price = 1.20m, Category = "Juice", Stock = 150, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-90), ImageUrl = "https://images.unsplash.com/photo-1613478223719-2ab802602423?auto=format&fit=crop&w=400&h=400" },
                new Product { Name = "Iced Coffee Latte", Code = "DRK-002", Price = 2.50m, Category = "Juice", Stock = 100, IsActive = true, DateIntroduced = DateTime.Now.AddDays(-1), ImageUrl = "https://images.unsplash.com/photo-1517701604599-bb29b565090c?auto=format&fit=crop&w=400&h=400" }
            };

            foreach (Product p in products)
            {
                if (!context.Products.Any(dbP => dbP.Code == p.Code))
                {
                    context.Products.Add(p);
                }
            }
            context.SaveChanges();

            // Add Admin user if not exists
            if (!context.Users.Any(u => u.Name == "Admin"))
            {
                context.Users.Add(new User
                {
                    Name = "Admin",
                    Role = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    IsActive = true,
                    DateCreated = DateTime.Now,
                    Age = 30,
                    Sex = "Other",
                    ImageUrl = "https://ui-avatars.com/api/?name=Admin&background=4F46E5&color=fff"
                });
                context.SaveChanges();
            }
        }
    }
}
