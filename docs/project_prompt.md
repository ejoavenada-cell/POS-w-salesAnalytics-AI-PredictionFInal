# POS System with AI Analytics - Master Prompt

## System Overview
A production-grade POS Food Ordering System built with ASP.NET Core MVC (C#) and an integrated AI Analytics layer.

## Core Requirements

### 1. Authentication & Users
- **Fields**: Name (200), Age, Sex (20), Role (Admin/Staff), PasswordHash, IsActive, DateCreated.
- **Security**: BCrypt hashing, server-side validation.
- **Default Account**: Admin (Password: Admin@123).
- **Admin Management**: Search, Edit, Enable/Disable, Approve users.

### 2. Product Management
- **Fields**: Code, Name, Price, Stock (w/ Restock), Category (Meals, Drinks, Sides, Desserts), Discount %, Date Introduced.
- **Delete Logic**: Must type the product name to confirm deletion.
- **Rules**: 
  - "NEW" if DateIntroduced is within 30 days of today.
  - "TOP SELLING" based on sales volume.

### 3. POS Terminal (Staff)
- **Grid View**: Category tabs + Search.
- **Badges**: 'NEW' (Top Right), 'Flame Icon' (Top Left for Top Selling).
- **Pricing**: Struck-through original price (Red) + active discounted price.
- **Checkout**:
  - Add/Edit quantities.
  - VAT (12%) calculation.
  - Bill Buttons: 50, 100, 200, 500.
  - Receipt ID: RCK-JHN-1000+ format.

### 4. AI & Analytics
- **Visuals**: Pie charts (Sales by Category), Line charts (Revenue Trends).
- **AI Analytics**: Predictive sales forecasting.
- **Filters**: ID search, Weekly/Monthly trends, Datepicker.
