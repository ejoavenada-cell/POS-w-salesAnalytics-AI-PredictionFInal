namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class ProductDisplayViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public bool HasDiscount { get; set; }
        public bool IsNew { get; set; }
        public bool IsTopSelling { get; set; }
        public decimal? Stock { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal DiscountPercent { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Subtotal => (Price * Quantity) * (1 - (DiscountPercent / 100));
    }

    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal TotalAmount => Items.Sum(i => i.Subtotal);
        public decimal TaxAmount => TotalAmount * 0.12m; // 12% VAT
        public decimal GrandTotal => TotalAmount + TaxAmount;
    }
}
