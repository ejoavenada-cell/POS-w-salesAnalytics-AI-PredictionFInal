using FoodOrderingSytemAIAnalytics.Models;

namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public User? User { get; set; }
        public string? Role => User?.Role;
    }
}
