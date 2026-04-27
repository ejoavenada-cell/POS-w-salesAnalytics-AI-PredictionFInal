using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SettingsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var settings = await _context.StoreSettings.FirstOrDefaultAsync() ?? new StoreSetting();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(StoreSetting settings)
        {
            var dbSettings = await _context.StoreSettings.FirstOrDefaultAsync();
            if (dbSettings == null)
            {
                dbSettings = new StoreSetting();
                _context.StoreSettings.Add(dbSettings);
            }

            if (settings.LogoFile != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "branding");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = "logo_" + Guid.NewGuid().ToString().Substring(0, 8) + Path.GetExtension(settings.LogoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await settings.LogoFile.CopyToAsync(fileStream);
                }
                dbSettings.LogoUrl = "/uploads/branding/" + uniqueFileName;
            }

            dbSettings.StoreName = settings.StoreName;
            dbSettings.CurrencySymbol = settings.CurrencySymbol;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Settings updated successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
