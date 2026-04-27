namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class SalesTrendViewModel
    {
        public string TimeLabel { get; set; } = string.Empty; // Date or Month name
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class CategoryPerformanceViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int TotalSold { get; set; }
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

    public class DashboardAnalyticsViewModel
    {
        public List<SalesTrendViewModel> DailyTrends { get; set; } = new List<SalesTrendViewModel>();
        public List<SalesTrendViewModel> ForecastTrends { get; set; } = new List<SalesTrendViewModel>();
        public List<ProductPerformanceViewModel> TopProducts { get; set; } = new List<ProductPerformanceViewModel>();
        public List<ProductPerformanceViewModel> BottomProducts { get; set; } = new List<ProductPerformanceViewModel>();
        public List<CategoryPerformanceViewModel> CategoryData { get; set; } = new List<CategoryPerformanceViewModel>();
        public GrowthRateViewModel MonthlyGrowth { get; set; } = new GrowthRateViewModel();
        public List<string> AIInsights { get; set; } = new List<string>();
        public string? LastAISync { get; set; }
    }

    public class ProphetForecastItem
    {
        public string Ds { get; set; } = string.Empty;
        public double Yhat { get; set; }
    }
}
