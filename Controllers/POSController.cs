using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Services;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using System.Threading.Tasks;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize]
    public class POSController : Controller
    {
        private readonly IProductService _productService;
        private readonly IPOSService _posService;
        private readonly IAnalyticsService _analyticsService;

        public POSController(IProductService productService, IPOSService posService, IAnalyticsService analyticsService)
        {
            _productService = productService;
            _posService = posService;
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index(string? category, int page = 1)
        {
            var products = await _productService.GetProductsAsync(page, 20, category, null);
            var categories = await _productService.GetCategoriesAsync();
            var cart = _posService.GetCart();

            var viewModel = new POSTerminalViewModel
            {
                Products = products,
                Categories = categories,
                Cart = cart,
                SelectedCategory = category
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var result = await _posService.AddItemToCartAsync(productId, quantity);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            var result = await _posService.UpdateQuantityAsync(productId, quantity);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var result = await _posService.RemoveItemFromCartAsync(productId);
            return Json(new { success = result.Success, message = result.Message });
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(decimal cashReceived)
        {
            var cart = _posService.GetCart();
            if (!cart.Items.Any()) return Json(new { success = false, message = "Cart is empty." });

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Json(new { success = false, message = "User not identified." });
            
            var userId = int.Parse(userIdClaim.Value); 
            var result = await _posService.CheckoutAsync(userId, cashReceived);

            if (result.Success)
            {
                return Json(new { 
                    success = true, 
                    message = "Order placed successfully!", 
                    receiptId = result.ReceiptId,
                    total = cart.GrandTotal,
                    subtotal = cart.TotalAmount,
                    tax = cart.TaxAmount,
                    cash = cashReceived,
                    change = cashReceived - cart.GrandTotal,
                    items = cart.Items.Select(i => new {
                        name = i.ProductName,
                        qty = i.Quantity,
                        price = i.Price,
                        subtotal = i.Subtotal
                    })
                });
            }

            return Json(new { success = false, message = result.Message });
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            var cart = _posService.GetCart();
            return PartialView("_CartPartial", cart);
        }
    }
}
