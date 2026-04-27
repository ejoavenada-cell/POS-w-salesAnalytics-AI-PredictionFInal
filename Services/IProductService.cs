using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDisplayViewModel>> GetProductsAsync(int page = 1, int pageSize = 20, string? category = null, string? searchTerm = null);
        Task<ProductDisplayViewModel?> GetProductDisplayByIdAsync(int id);
        Task<Product?> GetProductByIdAsync(int id);
        Task<(bool Success, string Message, Product? Product)> AddProductAsync(Product product);
        Task<(bool Success, string Message)> UpdateProductAsync(Product product);
        Task<(bool Success, string Message)> SoftDeleteProductAsync(int id);
        Task<(bool Success, string Message)> UpdateStockAsync(int productId, int quantityChange);
        Task<(bool Success, string Message)> ApplyDiscountAsync(int productId, decimal discountPercent);
        Task<bool> IsStockAvailableAsync(int productId, int requestedQuantity);
        
        // Category Management
        Task<IEnumerable<Category>> GetCategoriesAsync();
        Task<(bool Success, string Message)> AddCategoryAsync(Category category);
    }
}
