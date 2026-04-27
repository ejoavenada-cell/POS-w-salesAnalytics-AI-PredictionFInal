using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using System.Security.Claims;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterViewModel model);
        Task<AuthResult> LoginAsync(LoginViewModel model);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        ClaimsPrincipal CreatePrincipal(User user);
    }
}
