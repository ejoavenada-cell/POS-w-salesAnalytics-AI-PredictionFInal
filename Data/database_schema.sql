/*
================================================================================
POS Food Ordering System with AI Analytics - Database Schema
Author: Antigravity (Senior Software Architect)
Date: 2026-04-24
Normalization: 3rd Normal Form (3NF)
Target: SQL Server
================================================================================
*/

USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'FoodOrderingDB')
BEGIN
    ALTER DATABASE FoodOrderingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FoodOrderingDB;
END
GO

CREATE DATABASE FoodOrderingDB;
GO

USE FoodOrderingDB;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Create Users Table
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Age INT NULL,
    Sex NVARCHAR(10) NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Staff')),
    PasswordHash NVARCHAR(MAX) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    DateCreated DATETIME NOT NULL DEFAULT GETDATE()
);

-- 2. Create Products Table
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(50) UNIQUE NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
    Stock INT NULL,
    Category NVARCHAR(50) NOT NULL CHECK (Category IN ('Meals', 'Drinks', 'Sides', 'Desserts')),
    DiscountPercent DECIMAL(5, 2) NOT NULL DEFAULT 0,
    DateIntroduced DATETIME NOT NULL DEFAULT GETDATE(),
    IsActive BIT NOT NULL DEFAULT 1
);

-- 3. Create Transactions Table
CREATE TABLE Transactions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TransactionCode NVARCHAR(50) UNIQUE NOT NULL,
    UserId INT NOT NULL,
    Date DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18, 2) NOT NULL,
    CashReceived DECIMAL(18, 2) NOT NULL,
    Change DECIMAL(18, 2) NOT NULL,
    CONSTRAINT FK_Transactions_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 4. Create TransactionDetails Table
CREATE TABLE TransactionDetails (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TransactionId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18, 2) NOT NULL, -- Captured at time of sale
    Subtotal AS (Quantity * Price) PERSISTED, -- Persisted for AI/Reporting performance
    CONSTRAINT FK_TransactionDetails_Transactions FOREIGN KEY (TransactionId) REFERENCES Transactions(Id) ON DELETE CASCADE,
    CONSTRAINT FK_TransactionDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

-- ================================================================================
-- INDEXING FOR PERFORMANCE AND AI ANALYTICS
-- ================================================================================

-- Fast lookup for transactions by code
CREATE INDEX IX_Transactions_TransactionCode ON Transactions(TransactionCode);

-- Product lookup by name (common for POS search)
CREATE INDEX IX_Products_Name ON Products(Name);

-- Temporal indexing for AI trend analysis (Sales per day/week/month)
CREATE INDEX IX_Transactions_Date ON Transactions(Date);

-- Foreign Key indexing (Speeds up joins)
CREATE INDEX IX_Transactions_UserId ON Transactions(UserId);
CREATE INDEX IX_TransactionDetails_TransactionId ON TransactionDetails(TransactionId);
CREATE INDEX IX_TransactionDetails_ProductId ON TransactionDetails(ProductId);

-- ================================================================================
-- SAMPLE SEED DATA
-- ================================================================================

-- Seed Users
INSERT INTO Users (Name, Age, Sex, Role, PasswordHash, IsActive, DateCreated)
VALUES 
('Admin User', 35, 'Male', 'Admin', 'AQAAAAEAACcQAAAAE...', 1, GETDATE()), -- Example Hash
('Staff John', 25, 'Male', 'Staff', 'AQAAAAEAACcQAAAAE...', 1, GETDATE()),
('Staff Mary', 28, 'Female', 'Staff', 'AQAAAAEAACcQAAAAE...', 1, GETDATE());

-- Seed Products
INSERT INTO Products (Code, Name, Price, Stock, Category, DiscountPercent, DateIntroduced, IsActive)
VALUES 
('P-001', 'Signature Burger', 12.99, 100, 'Meals', 0, GETDATE(), 1),
('P-002', 'Crispy Fries', 4.50, 200, 'Sides', 0, GETDATE(), 1),
('P-003', 'Coca Cola', 2.50, 150, 'Drinks', 0, GETDATE(), 1),
('P-004', 'Chocolate Sundae', 5.99, 50, 'Desserts', 10, GETDATE(), 1),
('P-005', 'Chicken Nuggets', 8.00, 80, 'Meals', 0, GETDATE(), 1);

-- Seed Transactions (Example codes RCK-JHN-1001, 1002)
-- Transaction 1
INSERT INTO Transactions (TransactionCode, UserId, Date, TotalAmount, CashReceived, Change)
VALUES ('RCK-JHN-1001', 2, GETDATE(), 17.49, 20.00, 2.51);

-- Transaction Details 1
INSERT INTO TransactionDetails (TransactionId, ProductId, Quantity, Price)
VALUES 
(1, 1, 1, 12.99), -- Burger
(1, 2, 1, 4.50);  -- Fries

-- Transaction 2
INSERT INTO Transactions (TransactionCode, UserId, Date, TotalAmount, CashReceived, Change)
VALUES ('RCK-JHN-1002', 3, GETDATE(), 13.99, 15.00, 1.01);

-- Transaction Details 2
INSERT INTO TransactionDetails (TransactionId, ProductId, Quantity, Price)
VALUES 
(2, 5, 1, 8.00), -- Chicken Nuggets
(2, 4, 1, 5.99); -- Chocolate Sundae (with 10% discount handled by logic/final price)

GO
