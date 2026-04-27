using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductService productService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? category, int page = 1)
        {
            var products = await _productService.GetProductsAsync(page, 20, category, searchTerm);
            ViewBag.SearchTerm = searchTerm;
            ViewBag.Category = category;
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var product = new Product();
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_CreateProductPartial", product);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    product.ImageUrl = await SaveImage(product.ImageFile);
                }

                var result = await _productService.AddProductAsync(product);
                if (result.Success) return RedirectToAction("Products", "Admin");
                ModelState.AddModelError("", result.Message);
            }
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(product);
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/products/" + uniqueFileName;
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            ViewBag.Categories = await _productService.GetCategoriesAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView("_EditProductPartial", product);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                if (product.ImageFile != null)
                {
                    product.ImageUrl = await SaveImage(product.ImageFile);
                }

                var result = await _productService.UpdateProductAsync(product);
                if (result.Success) return RedirectToAction("Products", "Admin");
                ModelState.AddModelError("", result.Message);
            }
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string confirmName)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null) return NotFound();

            if (product.Name != confirmName)
            {
                TempData["ErrorMessage"] = "Product name does not match. Deletion cancelled.";
                return RedirectToAction("Products", "Admin");
            }

            var result = await _productService.SoftDeleteProductAsync(id);
            if (result.Success) TempData["SuccessMessage"] = "Product deleted successfully.";
            else TempData["ErrorMessage"] = result.Message;

            return RedirectToAction("Products", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> Restock(int productId, int quantity)
        {
            var result = await _productService.UpdateStockAsync(productId, quantity);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
