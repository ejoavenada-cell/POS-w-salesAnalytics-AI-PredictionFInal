using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public class POSService : IPOSService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionIdGenerator _transactionIdGenerator;
        private const string CartSessionKey = "POS_Cart";
        private static readonly Random random = new Random();

        public POSService(
            ApplicationDbContext context, 
            IProductService productService, 
            IHttpContextAccessor httpContextAccessor,
            ITransactionIdGenerator transactionIdGenerator)
        {
            _context = context;
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
            _transactionIdGenerator = transactionIdGenerator;
        }

        public CartViewModel GetCart()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return new CartViewModel();

            var cartJson = session.GetString(CartSessionKey);
            return cartJson == null ? new CartViewModel() : JsonSerializer.Deserialize<CartViewModel>(cartJson) ?? new CartViewModel();
        }

        private void SaveCart(CartViewModel cart)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetString(CartSessionKey, JsonSerializer.Serialize(cart));
            }
        }

        public async Task<(bool Success, string Message)> AddItemToCartAsync(int productId, int quantity)
        {
            if (quantity <= 0) return (false, "Quantity must be greater than zero.");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) return (false, "Product not found.");

            if (!await _productService.IsStockAvailableAsync(productId, quantity))
                return (false, "Insufficient stock.");

            var cart = GetCart();
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (existingItem != null)
            {
                if (!await _productService.IsStockAvailableAsync(productId, existingItem.Quantity + quantity))
                    return (false, "Insufficient stock for total quantity.");
                
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItemViewModel
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    DiscountPercent = product.DiscountPercent,
                    ImageUrl = product.ImageUrl
                });
            }

            SaveCart(cart);
            return (true, "Item added to cart.");
        }

        public async Task<(bool Success, string Message)> UpdateQuantityAsync(int productId, int quantity)
        {
            if (quantity <= 0) return await RemoveItemFromCartAsync(productId);

            if (!await _productService.IsStockAvailableAsync(productId, quantity))
                return (false, "Insufficient stock.");

            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return (false, "Item not found in cart.");

            item.Quantity = quantity;
            SaveCart(cart);
            return (true, "Quantity updated.");
        }

        public async Task<(bool Success, string Message)> RemoveItemFromCartAsync(int productId)
        {
            var cart = GetCart();
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return (false, "Item not found in cart.");

            cart.Items.Remove(item);
            SaveCart(cart);
            return (true, "Item removed from cart.");
        }

        public void ClearCart()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(CartSessionKey);
        }

        public async Task<(bool Success, string Message, string? ReceiptId)> CheckoutAsync(int userId, decimal cashReceived)
        {
            var cart = GetCart();
            if (!cart.Items.Any()) return (false, "Cart is empty.", null);

            if (cashReceived < cart.GrandTotal)
                return (false, $"Insufficient cash. Total is {cart.GrandTotal:C2}.", null);

            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                using var transactionScope = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Generate Receipt ID
                    var receiptId = await _transactionIdGenerator.GenerateNextIdAsync();

                    // Handle Virtual Admin Transaction Ownership
                    var actualUserId = userId;
                    if (userId == -999)
                    {
                        var fallbackAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Role == "Admin" && u.Id > 0);
                        if (fallbackAdmin != null) actualUserId = fallbackAdmin.Id;
                    }

                    // 2. Create Transaction record
                    var transaction = new Transaction
                    {
                        TransactionCode = receiptId,
                        UserId = actualUserId,
                        Date = DateTime.Now,
                        TotalAmount = cart.GrandTotal,
                        CashReceived = cashReceived,
                        Change = cashReceived - cart.GrandTotal,
                        IsWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday
                    };

                    _context.Transactions.Add(transaction);
                    // Don't SaveChanges yet, do it all at once

                    // 3. Create Details and Update Stock
                    foreach (var item in cart.Items)
                    {
                        // Atomic Stock Check and Reduction (Delayed Save)
                        var stockResult = await _productService.UpdateStockAsync(item.ProductId, -item.Quantity, false);
                        if (!stockResult.Success)
                        {
                            throw new InvalidOperationException($"Stock error for {item.ProductName}: {stockResult.Message}");
                        }

                        var detail = new TransactionDetail
                        {
                            Transaction = transaction, // Link via object instead of ID before save
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        _context.TransactionDetails.Add(detail);
                    }

                    // FINAL ATOMIC SAVE
                    await _context.SaveChangesAsync();
                    await transactionScope.CommitAsync();

                    ClearCart();
                    return (true, "Checkout successful.", receiptId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await transactionScope.RollbackAsync();
                    if (attempt == maxRetries)
                    {
                        return (false, "System is busy with multiple orders. Please try again in a moment.", null);
                    }
                    // Wait a bit before retrying (exponential backoff)
                    await Task.Delay(random.Next(50, 150) * attempt);
                }
                catch (DbUpdateException ex)
                {
                    await transactionScope.RollbackAsync();
                    // Check for Unique Constraint violation or other DB errors
                    if (attempt == maxRetries)
                    {
                        var innerMsg = ex.InnerException?.Message ?? ex.Message;
                        return (false, $"Database error: {innerMsg}", null);
                    }
                    // Wait a bit and retry
                    await Task.Delay(random.Next(50, 150) * attempt);
                }
                catch (InvalidOperationException ex)
                {
                    await transactionScope.RollbackAsync();
                    return (false, ex.Message, null);
                }
                catch (Exception ex)
                {
                    await transactionScope.RollbackAsync();
                    return (false, $"Checkout failed: {ex.Message}", null);
                }
            }
            return (false, "Checkout failed.", null);
        }

        // Removed GenerateReceiptIdAsync because it's now handled by ITransactionIdGenerator
    }
}
