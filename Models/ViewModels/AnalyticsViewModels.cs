namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class SalesTrendViewModel
    {
        public string TimeLabel { get; set; } = string.Empty; // Date or Month name
        public decimal Revenue { get; set; }
        public decimal OrderCount { get; set; }
    }

    public class CategoryPerformanceViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal TotalSold { get; set; }
        public double PercentageOfTotalSales { get; set; }
    }

    public class GrowthRateViewModel
    {
        public decimal CurrentRevenue { get; set; }
        public decimal PreviousRevenue { get; set; }
        public double GrowthPercentage { get; set; }
        public string PeriodLabel { get; set; } = string.Empty;
    }

    public class PredictiveDataViewModel
    {
        public DateTime Date { get; set; }
        public decimal HistoricalRevenue { get; set; }
        public decimal? PredictedRevenue { get; set; }
        public string TrendIndication { get; set; } = "Stable"; // Up/Down/Stable
    }

    public class LowStockItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal CurrentStock { get; set; }
        public decimal RecommendedRestock { get; set; }
        public double StockPercentage => Math.Min(100, (double)(CurrentStock / 500.0m) * 100); // 500 is the max stock level
    }

    public class StockAnalyticsViewModel
    {
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public List<LowStockItemViewModel> LowStockItems { get; set; } = new();
        public List<LowStockItemViewModel> AllStockItems { get; set; } = new();
        
        public double WeeklyUsageChange { get; set; } // % change in items sold vs previous week
        public double MonthlyUsageChange { get; set; } // % change in items sold vs previous month
        public decimal TotalItemsSoldWeekly { get; set; }
        public decimal TotalItemsSoldMonthly { get; set; }
        public List<decimal> MonthlyWeeklyUsage { get; set; } = new();
        public string MonthlyUsageInsight { get; set; } = string.Empty;
        public double AverageWeeklyGrowth { get; set; }
        public List<SalesTrendViewModel> YearlyMonthlyTrend { get; set; } = new();
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }
    }

    public class DashboardAnalyticsViewModel
    {
        public List<SalesTrendViewModel> DailyTrends { get; set; } = new List<SalesTrendViewModel>();
        public List<SalesTrendViewModel> ForecastTrends { get; set; } = new List<SalesTrendViewModel>();
        public List<ProductPerformanceViewModel> TopProducts { get; set; } = new List<ProductPerformanceViewModel>();
        public List<ProductPerformanceViewModel> BottomProducts { get; set; } = new List<ProductPerformanceViewModel>();
        public List<CategoryPerformanceViewModel> CategoryData { get; set; } = new List<CategoryPerformanceViewModel>();
        public GrowthRateViewModel MonthlyGrowth { get; set; } = new GrowthRateViewModel();
        public StockAnalyticsViewModel StockSummary { get; set; } = new StockAnalyticsViewModel();
        public List<string> AIInsights { get; set; } = new List<string>();
        public string? LastAISync { get; set; }
        public double ConfidenceScore { get; set; }
        public string TrendVelocity { get; set; } = string.Empty;
        public string ChurnRisk { get; set; } = string.Empty;
        public List<int> AccuracyTrend { get; set; } = new();
        public List<ProphetForecastItem> ForecastData { get; set; } = new();
        public StockLifeCycleViewModel StockLifeCycle { get; set; } = new();
    }

    public class ProphetForecastItem
    {
        public DateTime Date { get; set; }
        public decimal PredictedRevenue { get; set; }
        public string Confidence { get; set; } = "High";
    }

    public class StockLifeCyclePoint
    {
        public DateTime Date { get; set; }
        public decimal StockLevel { get; set; }
        public bool IsRestock { get; set; }
    }

    public class StockProductSeries
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Color { get; set; } = "#2D9F96";
        public List<StockLifeCyclePoint> Points { get; set; } = new();
    }

    public class StockLifeCycleViewModel
    {
        public List<StockProductSeries> Series { get; set; } = new();
        public List<string> AllLabels { get; set; } = new(); // Dates
    }
}
