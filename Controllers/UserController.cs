using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using FoodOrderingSytemAIAnalytics.Services;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _webHostEnvironment;

        public UserController(ApplicationDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(string? searchTerm)
        {
            var users = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                users = users.Where(u => u.Name.Contains(searchTerm));
            }
            ViewBag.SearchTerm = searchTerm;
            return View(await users.ToListAsync());
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EditUserPartial", user);

            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user, string? NewPassword)
        {
            var dbUser = await _context.Users.FindAsync(user.Id);
            if (dbUser == null) return NotFound();

            // Handle Image Upload
            if (user.ProfileImage != null)
            {
                dbUser.ImageUrl = await SaveProfileImage(user.ProfileImage);
            }
            else if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                dbUser.ImageUrl = user.ImageUrl;
            }

            // Update allowed fields
            dbUser.Name = user.Name;
            dbUser.Age = user.Age;
            dbUser.Sex = user.Sex;
            dbUser.IsActive = user.IsActive;

            // Handle Password Update if provided
            if (!string.IsNullOrEmpty(NewPassword))
            {
                dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "User was modified by another process. Please refresh.");
            }

            return View(user);
        }

        private async Task<string> SaveProfileImage(Microsoft.AspNetCore.Http.IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
            if (!System.IO.Directory.Exists(uploadsFolder)) System.IO.Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/profiles/" + uniqueFileName;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            ViewBag.IsSelfEdit = true;
            return PartialView("_EditUserPartial", user);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(User user, string? NewPassword)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (user.Id != userId) return Forbid();

            var dbUser = await _context.Users.FindAsync(userId);
            if (dbUser == null) return NotFound();

            if (user.ProfileImage != null)
            {
                dbUser.ImageUrl = await SaveProfileImage(user.ProfileImage);
            }
            else if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                dbUser.ImageUrl = user.ImageUrl;
            }

            dbUser.Name = user.Name;
            dbUser.Age = user.Age;
            dbUser.Sex = user.Sex;

            if (!string.IsNullOrEmpty(NewPassword))
            {
                dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            await _context.SaveChangesAsync();

            // REFRESH COOKIE to update claims (like ImageUrl) in layout
            var authService = HttpContext.RequestServices.GetRequiredService<IAuthService>();
            var principal = authService.CreatePrincipal(dbUser);
            await HttpContext.SignInAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
