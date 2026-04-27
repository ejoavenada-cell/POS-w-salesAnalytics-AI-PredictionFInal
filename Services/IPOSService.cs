using FoodOrderingSytemAIAnalytics.Models.ViewModels;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface IPOSService
    {
        CartViewModel GetCart();
        Task<(bool Success, string Message)> AddItemToCartAsync(int productId, int quantity);
        Task<(bool Success, string Message)> UpdateQuantityAsync(int productId, int quantity);
        Task<(bool Success, string Message)> RemoveItemFromCartAsync(int productId);
        void ClearCart();
        Task<(bool Success, string Message, string? ReceiptId)> CheckoutAsync(int userId, decimal cashReceived);
    }
}
