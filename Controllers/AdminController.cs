using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Services;
using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using MiniExcelLibs;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.IO.Compression;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(IAnalyticsService analyticsService, ITransactionService transactionService, IProductService productService, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _analyticsService = analyticsService;
            _transactionService = transactionService;
            _productService = productService;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string period = "7", int? month = null, int? year = null)
        {
            var model = await _analyticsService.GetDashboardSummaryAsync(period, month, year);
            ViewBag.CurrentPeriod = period;
            return View(model);
        }

        public async Task<IActionResult> DetailedReport()
        {
            var model = await _analyticsService.GetDashboardSummaryAsync("30");
            return View(model);
        }

        public async Task<IActionResult> Products(string? category)
        {
            var products = await _productService.GetProductsAsync(1, 100, category, null, false);
            ViewBag.SelectedCategory = category;
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _productService.ToggleProductStatusAsync(id);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(FoodOrderingSytemAIAnalytics.Models.Category category)
        {
            var result = await _productService.AddCategoryAsync(category);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> GetStockLifeCycle(string? category, string? productName, DateTime? startDate, DateTime? endDate)
        {
            int? productId = null;
            if (!string.IsNullOrEmpty(productName))
            {
                var products = await _productService.GetProductsAsync(1, 1, null, productName);
                productId = products.FirstOrDefault()?.Id;
            }

            var data = await _analyticsService.GetStockLifeCycleAsync(productId, category, startDate, endDate);
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> RestockProduct([FromForm] int productId, [FromForm] decimal quantity)
        {
            var result = await _productService.UpdateStockAsync(productId, quantity);
            return Json(new { success = result.Success, message = result.Message });
        }
        [HttpGet]
        public async Task<IActionResult> DownloadTemplate()
        {
            var categories = await _context.Categories.Select(c => c.Name).ToListAsync();
            var categoryList = string.Join(", ", categories);
            
            var template = new[]
            {
                new { 
                    Code = "P-101", 
                    Name = "Sample Dish Name", 
                    Price = 150.00, 
                    Stock = 50.0, 
                    Category = categories.FirstOrDefault() ?? "Meals", 
                    ImageUrl = "https://link-to-your-image.com/dish.jpg", 
                    DiscountPercent = 0.0 
                }
            };

            // We can also add a note about categories in a second row if we want, 
            // but MiniExcel saves exactly what we pass. 
            // I'll stick to a clean first row.
            
            var stream = new MemoryStream();
            stream.SaveAs(template);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "POS_Product_Import_Template.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> ImportProducts(IFormFile excelFile, IFormFile? zipFile, string mode = "add")
        {
            if (excelFile == null || excelFile.Length == 0)
                return Json(new { success = false, message = "Please upload a valid Excel file." });

            var extension = Path.GetExtension(excelFile.FileName).ToLower();
            if (extension != ".xlsx" && extension != ".xls" && extension != ".csv")
                return Json(new { success = false, message = "Only .xlsx, .xls and .csv files are supported." });

            try
            {
                Dictionary<string, string> imageMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string tempPath = "";

                // 1. Process ZIP if provided
                if (zipFile != null && zipFile.Length > 0)
                {
                    tempPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "temp_" + Guid.NewGuid().ToString().Substring(0, 8));
                    string productsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                    
                    if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                    if (!Directory.Exists(productsFolder)) Directory.CreateDirectory(productsFolder);

                    string zipFilePath = Path.Combine(tempPath, zipFile.FileName);
                    using (var stream = new FileStream(zipFilePath, FileMode.Create))
                    {
                        await zipFile.CopyToAsync(stream);
                    }

                    ZipFile.ExtractToDirectory(zipFilePath, tempPath);

                    var files = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories)
                                         .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg") || f.EndsWith(".webp") || f.EndsWith(".avif"));

                    foreach (var file in files)
                    {
                        string originalName = Path.GetFileName(file);
                        string newFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + originalName;
                        string destPath = Path.Combine(productsFolder, newFileName);
                        
                        System.IO.File.Move(file, destPath);
                        imageMap[originalName] = "/uploads/products/" + newFileName;
                    }
                }

                // 2. Process Excel
                using (var stream = excelFile.OpenReadStream())
                {
                    var rows = stream.Query<ProductImportRow>().ToList();
                    
                    if (mode == "replace")
                    {
                        _context.RestockHistory.RemoveRange(_context.RestockHistory);
                        var products = await _context.Products.ToListAsync();
                        foreach(var p in products) {
                            var hasSales = await _context.TransactionDetails.AnyAsync(td => td.ProductId == p.Id);
                            if (!hasSales) _context.Products.Remove(p);
                            else p.IsActive = false;
                        }
                    }

                    int importedCount = 0;
                    int updatedCount = 0;

                    foreach (var row in rows)
                    {
                        if (string.IsNullOrWhiteSpace(row.Code) || string.IsNullOrWhiteSpace(row.Name)) continue;

                        // Resolve Image Path
                        string resolvedImageUrl = "/images/default.png";
                        if (!string.IsNullOrEmpty(row.ImageUrl))
                        {
                            // Check if the ImageUrl in Excel matches a file we just extracted
                            string fileNameOnly = Path.GetFileName(row.ImageUrl);
                            if (imageMap.ContainsKey(fileNameOnly))
                            {
                                resolvedImageUrl = imageMap[fileNameOnly];
                            }
                            else if (row.ImageUrl.StartsWith("http") || row.ImageUrl.StartsWith("/"))
                            {
                                resolvedImageUrl = row.ImageUrl; // Keep existing URL if it's already a link
                            }
                        }

                        var existing = await _context.Products.FirstOrDefaultAsync(p => p.Code == row.Code);
                        if (existing != null)
                        {
                            existing.Name = row.Name;
                            existing.Price = row.Price;
                            existing.Stock = row.Stock;
                            existing.Category = row.Category;
                            existing.ImageUrl = resolvedImageUrl;
                            existing.DiscountPercent = row.DiscountPercent;
                            existing.IsActive = true;
                            updatedCount++;
                        }
                        else
                        {
                            _context.Products.Add(new Product
                            {
                                Code = row.Code,
                                Name = row.Name,
                                Price = row.Price,
                                Stock = row.Stock,
                                Category = row.Category,
                                ImageUrl = resolvedImageUrl,
                                DiscountPercent = row.DiscountPercent,
                                DateIntroduced = DateTime.Now,
                                IsActive = true
                            });
                            importedCount++;
                        }
                    }

                    await _context.SaveChangesAsync();
                    
                    // Cleanup temp folder
                    if (!string.IsNullOrEmpty(tempPath) && Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);

                    return Json(new { success = true, message = $"Import successful! Processed {rows.Count} rows. ({importedCount} new, {updatedCount} updated)." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing import: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkUploadImages(IFormFile zipFile)
        {
            if (zipFile == null || zipFile.Length == 0)
                return Json(new { success = false, message = "Please upload a valid ZIP file." });

            if (Path.GetExtension(zipFile.FileName).ToLower() != ".zip")
                return Json(new { success = false, message = "Only .zip files are supported." });

            try
            {
                string tempPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "temp_" + Guid.NewGuid().ToString().Substring(0, 8));
                string productsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
                
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
                if (!Directory.Exists(productsFolder)) Directory.CreateDirectory(productsFolder);

                string zipPath = Path.Combine(tempPath, zipFile.FileName);
                using (var stream = new FileStream(zipPath, FileMode.Create))
                {
                    await zipFile.CopyToAsync(stream);
                }

                ZipFile.ExtractToDirectory(zipPath, tempPath);

                int matchedCount = 0;
                var files = Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories)
                                     .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg") || f.EndsWith(".webp") || f.EndsWith(".avif"));

                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    string productCode = Path.GetFileNameWithoutExtension(fileName);

                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Code == productCode);
                    if (product != null)
                    {
                        string newFileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + fileName;
                        string destPath = Path.Combine(productsFolder, newFileName);
                        System.IO.File.Move(file, destPath);
                        
                        product.ImageUrl = "/uploads/products/" + newFileName;
                        matchedCount++;
                    }
                }

                await _context.SaveChangesAsync();
                Directory.Delete(tempPath, true);

                return Json(new { success = true, message = $"Successfully processed ZIP. Matched and updated {matchedCount} product images." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error unzipping file: " + ex.Message });
            }
        }
    }
}
