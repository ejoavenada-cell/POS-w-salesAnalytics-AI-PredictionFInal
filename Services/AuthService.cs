using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using BCrypt.Net;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AuthResult> RegisterAsync(RegisterViewModel model)
        {
            if (await _context.Users.AnyAsync(u => u.Name == model.Username))
            {
                return new AuthResult { Success = false, Message = "Username already exists." };
            }

            var user = new User
            {
                Name = model.Username,
                Age = model.Age,
                Sex = model.Sex,
                Role = model.Role,
                PasswordHash = HashPassword(model.Password),
                IsActive = true,
                DateCreated = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new AuthResult { Success = true, Message = "Registration successful.", User = user };
        }

        public async Task<AuthResult> LoginAsync(LoginViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Name == model.Name);

            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return new AuthResult { Success = false, Message = "Invalid username or password." };
            }

            if (!user.IsActive)
            {
                return new AuthResult { Success = false, Message = "User account is inactive." };
            }

            return new AuthResult { Success = true, Message = "Login successful.", User = user };
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            }
            catch
            {
                return false;
            }
        }

        public ClaimsPrincipal CreatePrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("Age", user.Age?.ToString() ?? ""),
                new Claim("Sex", user.Sex ?? ""),
                new Claim("ImageUrl", user.ImageUrl ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }
    }
}
