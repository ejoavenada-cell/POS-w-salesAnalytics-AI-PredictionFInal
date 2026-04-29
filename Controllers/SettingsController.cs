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
            dbSettings.PrimaryColor = settings.PrimaryColor;

            // AI Tuning
            dbSettings.AIForecastHorizonHours = settings.AIForecastHorizonHours;
            dbSettings.AISensitivity = settings.AISensitivity;
            dbSettings.AISeasonalityMode = settings.AISeasonalityMode;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Settings updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RetrainAI()
        {
            var settings = await _context.StoreSettings.FirstOrDefaultAsync() ?? new StoreSetting();
            
            try
            {
                // Prepare Python process
                string pythonPath = "python"; 
                string aiFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "AI");
                string scriptPath = Path.Combine(aiFolder, "train_prophet.py");
                
                // Format arguments with InvariantCulture to avoid comma/decimal issues
                string args = $"\"{scriptPath}\" " +
                             $"--horizon {settings.AIForecastHorizonHours} " +
                             $"--sensitivity {settings.AISensitivity.ToString(System.Globalization.CultureInfo.InvariantCulture)} " +
                             $"--mode {settings.AISeasonalityMode}";

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = aiFolder
                };

                using (var process = System.Diagnostics.Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        await process.WaitForExitAsync();
                        string output = await process.StandardOutput.ReadToEndAsync();
                        string error = await process.StandardError.ReadToEndAsync();
                        
                        if (process.ExitCode == 0)
                        {
                            settings.LastAISync = DateTime.Now;
                            _context.StoreSettings.Update(settings);
                            await _context.SaveChangesAsync();
                            
                            TempData["SuccessMessage"] = "AI Model retrained successfully with current fine-tuning parameters!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "AI Training Error: " + error;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "System Error: Could not launch AI engine. Ensure Python is installed. " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
