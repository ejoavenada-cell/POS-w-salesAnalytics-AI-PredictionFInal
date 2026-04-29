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
                context.Database.EnsureCreated();
            }
            else
            {
                Console.WriteLine(">>> DB: Brute-Force Initializing PostgreSQL Schema...");
                try 
                {
                    if (Environment.GetEnvironmentVariable("RESET_DB") == "true")
                    {
                        Console.WriteLine(">>> DB: RESET_DB is true. Dropping all tables...");
                        context.Database.ExecuteSqlRaw(@"
                            DROP TABLE IF EXISTS restockhistory CASCADE;
                            DROP TABLE IF EXISTS transactiondetails CASCADE;
                            DROP TABLE IF EXISTS transactions CASCADE;
                            DROP TABLE IF EXISTS products CASCADE;
                            DROP TABLE IF EXISTS categories CASCADE;
                            DROP TABLE IF EXISTS storesettings CASCADE;
                            DROP TABLE IF EXISTS users CASCADE;
                        ");
                    }

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
                            datecreated TIMESTAMP,
                            rowversion BYTEA
                        );
                        CREATE TABLE IF NOT EXISTS storesettings (
                            id SERIAL PRIMARY KEY,
                            storename TEXT,
                            currencysymbol TEXT,
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
                            discountpercent DECIMAL DEFAULT 0,
                            isactive BOOLEAN,
                            dateintroduced TIMESTAMP,
                            rowversion BYTEA
                        );
                        CREATE TABLE IF NOT EXISTS transactions (
                            id SERIAL PRIMARY KEY,
                            transactioncode TEXT UNIQUE NOT NULL,
                            userid INTEGER REFERENCES users(id),
                            date TIMESTAMP NOT NULL,
                            totalamount DECIMAL NOT NULL,
                            cashreceived DECIMAL NOT NULL,
                            change DECIMAL NOT NULL,
                            isweekend BOOLEAN,
                            rowversion BYTEA
                        );
                        CREATE TABLE IF NOT EXISTS transactiondetails (
                            id SERIAL PRIMARY KEY,
                            transactionid INTEGER REFERENCES transactions(id),
                            productid INTEGER REFERENCES products(id),
                            quantity DECIMAL NOT NULL,
                            price DECIMAL NOT NULL,
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
                    context.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">>> DB ERROR: {ex.Message}");
                }
            }
            
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
