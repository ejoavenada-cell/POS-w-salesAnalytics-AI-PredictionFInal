using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPricingService _pricingService;
        private readonly IAnalyticsService _analyticsService;

        public ProductService(ApplicationDbContext context, IPricingService pricingService, IAnalyticsService analyticsService)
        {
            _context = context;
            _pricingService = pricingService;
            _analyticsService = analyticsService;
        }

        public async Task<IEnumerable<ProductDisplayViewModel>> GetProductsAsync(int page = 1, int pageSize = 20, string? category = null, string? searchTerm = null)
        {
            var query = _context.Products.AsNoTracking().Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm));

            if (!string.IsNullOrEmpty(category) && category != "All")
                query = query.Where(p => p.Category == category);

            var products = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var topSelling = await _analyticsService.GetTopSellingProductsAsync(5);
            var topSellingIds = topSelling.Select(t => t.ProductId).ToHashSet();

            return products.Select(p => new ProductDisplayViewModel
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                OriginalPrice = p.Price,
                FinalPrice = _pricingService.CalculateFinalPrice(p.Price, p.DiscountPercent),
                HasDiscount = _pricingService.HasActiveDiscount(p.DiscountPercent),
                IsNew = (DateTime.Now - p.DateIntroduced).TotalDays <= 30,
                IsTopSelling = topSellingIds.Contains(p.Id),
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            });
        }

        public async Task<ProductDisplayViewModel?> GetProductDisplayByIdAsync(int id)
        {
            var p = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
            if (p == null) return null;

            var topSelling = await _analyticsService.GetTopSellingProductsAsync(5);
            var topSellingIds = topSelling.Select(t => t.ProductId).ToHashSet();

            return new ProductDisplayViewModel
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                OriginalPrice = p.Price,
                FinalPrice = _pricingService.CalculateFinalPrice(p.Price, p.DiscountPercent),
                HasDiscount = _pricingService.HasActiveDiscount(p.DiscountPercent),
                IsNew = (DateTime.Now - p.DateIntroduced).TotalDays <= 30,
                IsTopSelling = topSellingIds.Contains(p.Id),
                Stock = p.Stock,
                ImageUrl = p.ImageUrl
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<(bool Success, string Message, Product? Product)> AddProductAsync(Product product)
        {
            // Validations
            if (product.Price <= 0) return (false, "Price must be greater than zero.", null);
            if (product.Stock < 0) return (false, "Stock cannot be negative.", null);
            if (await _context.Products.AnyAsync(p => p.Code == product.Code)) 
                return (false, "Product code must be unique.", null);

            product.DateIntroduced = DateTime.Now;
            product.IsActive = true;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return (true, "Product added successfully.", product);
        }

        public async Task<(bool Success, string Message)> UpdateProductAsync(Product product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing == null) return (false, "Product not found.");

            // Validations
            if (product.Price <= 0) return (false, "Price must be greater than zero.");
            if (product.Stock < 0) return (false, "Stock cannot be negative.");
            
            // Ensure unique code if it changed
            if (existing.Code != product.Code && await _context.Products.AnyAsync(p => p.Code == product.Code))
                return (false, "Product code must be unique.");

            _context.Entry(existing).CurrentValues.SetValues(product);
            await _context.SaveChangesAsync();

            return (true, "Product updated successfully.");
        }

        public async Task<(bool Success, string Message)> SoftDeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return (false, "Product not found.");

            product.IsActive = false;
            await _context.SaveChangesAsync();

            return (true, "Product deactivated successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateStockAsync(int productId, int quantityChange)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return (false, "Product not found.");

            if (product.Stock + quantityChange < 0)
                return (false, "Insufficient stock.");

            product.Stock += quantityChange;
            await _context.SaveChangesAsync();

            return (true, "Stock updated successfully.");
        }

        public async Task<(bool Success, string Message)> ApplyDiscountAsync(int productId, decimal discountPercent)
        {
            if (discountPercent < 0 || discountPercent > 100)
                return (false, "Discount must be between 0 and 100.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null) return (false, "Product not found.");

            product.DiscountPercent = discountPercent;
            await _context.SaveChangesAsync();

            return (true, "Discount applied successfully.");
        }

        public async Task<bool> IsStockAvailableAsync(int productId, int requestedQuantity)
        {
            var product = await _context.Products.FindAsync(productId);
            return product != null && product.IsActive && product.Stock >= requestedQuantity;
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.Where(c => c.IsActive).ToListAsync();
        }

        public async Task<(bool Success, string Message)> AddCategoryAsync(Category category)
        {
            if (await _context.Categories.AnyAsync(c => c.Name == category.Name))
                return (false, "Category already exists.");

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return (true, "Category added successfully.");
        }
    }
}
