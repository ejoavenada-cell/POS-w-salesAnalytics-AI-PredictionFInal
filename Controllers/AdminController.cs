using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Services;
using System.Threading.Tasks;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;

        public AdminController(IAnalyticsService analyticsService, ITransactionService transactionService, IProductService productService)
        {
            _analyticsService = analyticsService;
            _transactionService = transactionService;
            _productService = productService;
        }

        public async Task<IActionResult> Index(string period = "7")
        {
            var model = await _analyticsService.GetDashboardSummaryAsync(period);
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
            var products = await _productService.GetProductsAsync(1, 100, category, null);
            ViewBag.SelectedCategory = category;
            ViewBag.Categories = await _productService.GetCategoriesAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(FoodOrderingSytemAIAnalytics.Models.Category category)
        {
            var result = await _productService.AddCategoryAsync(category);
            if (result.Success) TempData["SuccessMessage"] = result.Message;
            else TempData["ErrorMessage"] = result.Message;
            return RedirectToAction("Products");
        }
    }
}
