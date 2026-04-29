namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class TransactionSummaryViewModel
    {
        public int TransactionId { get; set; }
        public string ReceiptId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }

    public class SalesReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageOrderValue => TotalTransactions > 0 ? TotalRevenue / TotalTransactions : 0;
        public List<ProductPerformanceViewModel> TopProducts { get; set; } = new List<ProductPerformanceViewModel>();
    }

    public class ProductPerformanceViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public decimal QuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public double GrowthPercentage { get; set; }
    }
}
