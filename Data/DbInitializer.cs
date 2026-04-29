using FoodOrderingSytemAIAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FoodOrderingSytemAIAnalytics.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Database.IsSqlServer())
            {
                // SQL Server specific logic (omitted for brevity but kept in original)
                context.Database.EnsureCreated();
            }
            else
            {
                Console.WriteLine(">>> DB: Brute-Force Initializing PostgreSQL Schema...");
                try 
                {
                    // Manually create tables to ensure they exist with correct lowercase names
                    string manualSql = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id SERIAL PRIMARY KEY,
                            name TEXT UNIQUE NOT NULL,
                            passwordhash TEXT,
                            role TEXT,
                            age INTEGER,
                            sex TEXT,
                            isactive BOOLEAN,
                            isapproved BOOLEAN,
                            imageurl TEXT,
                            datecreated TIMESTAMP
                        );
                        CREATE TABLE IF NOT EXISTS storesettings (
                            id SERIAL PRIMARY KEY,
                            storename TEXT,
                            currencysymbol TEXT,
                            taxrate DECIMAL,
                            primarycolor TEXT,
                            logourl TEXT,
                            aiforecasthorizonhours INTEGER,
                            aisensitivity DOUBLE PRECISION,
                            aiseasonalitymode TEXT,
                            lastaisync TIMESTAMP
                        );
                        CREATE TABLE IF NOT EXISTS categories (
                            id SERIAL PRIMARY KEY,
                            name TEXT UNIQUE NOT NULL,
                            imageurl TEXT,
                            isactive BOOLEAN
                        );
                        CREATE TABLE IF NOT EXISTS products (
                            id SERIAL PRIMARY KEY,
                            code TEXT UNIQUE NOT NULL,
                            name TEXT NOT NULL,
                            description TEXT,
                            price DECIMAL NOT NULL,
                            stock DECIMAL,
                            category TEXT,
                            imageurl TEXT,
                            isactive BOOLEAN,
                            dateintroduced TIMESTAMP
                        );
                        CREATE TABLE IF NOT EXISTS transactions (
                            id SERIAL PRIMARY KEY,
                            transactionid TEXT UNIQUE,
                            userid INTEGER REFERENCES users(id),
                            totalamount DECIMAL,
                            discountamount DECIMAL,
                            finalamount DECIMAL,
                            paymentmethod TEXT,
                            transactiondate TIMESTAMP,
                            status TEXT
                        );
                        CREATE TABLE IF NOT EXISTS transactiondetails (
                            id SERIAL PRIMARY KEY,
                            transactionid INTEGER REFERENCES transactions(id),
                            productid INTEGER REFERENCES products(id),
                            quantity DECIMAL,
                            price DECIMAL,
                            subtotal DECIMAL
                        );
                        CREATE TABLE IF NOT EXISTS restockhistory (
                            id SERIAL PRIMARY KEY,
                            productid INTEGER REFERENCES products(id),
                            quantityrestocked DECIMAL,
                            restockdate TIMESTAMP,
                            remarks TEXT,
                            stockafterrestock DECIMAL
                        );";

                    context.Database.ExecuteSqlRaw(manualSql);
                    Console.WriteLine(">>> DB: Manual Schema Creation Finished.");
                    
                    // Now try to let EF Core fill in any blanks
                    context.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">>> DB ERROR during manual creation: {ex.Message}");
                }
            }
            
            // Seed Data
            try 
            {
                SeedStoreSettings(context);
                SeedProducts(context);
                Console.WriteLine(">>> DB: Seeding Complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> DB SEED ERROR: {ex.Message}");
            }
        }

        public static void SeedStoreSettings(ApplicationDbContext context)
        {
            if (!context.StoreSettings.Any())
            {
                context.StoreSettings.Add(new StoreSetting
                {
                    StoreName = "Tasty Station",
                    CurrencySymbol = "₱",
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
                new Product { Code = "P-003", Name = "Coca Cola", Price = 2.50m, Stock = 150, Category = "Drinks", IsActive = true, DateIntroduced = DateTime.Now.AddMonths(-3) }
            };
            context.Products.AddRange(seedProducts);
            context.SaveChanges();
        }
    }
}
