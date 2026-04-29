using FoodOrderingSytemAIAnalytics.Models.ViewModels;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface IAnalyticsService
    {
        Task<List<SalesTrendViewModel>> GetSalesTrendByDayAsync(int days = 7);
        Task<List<SalesTrendViewModel>> GetSalesTrendByMonthAsync(int months = 6);
        Task<List<ProductPerformanceViewModel>> GetTopSellingProductsAsync(int count = 5);
        Task<List<ProductPerformanceViewModel>> GetLeastSellingProductsAsync(int count = 5);
        Task<GrowthRateViewModel> GetRevenueGrowthRateAsync();
        Task<List<CategoryPerformanceViewModel>> GetCategoryPerformanceAsync();
        Task<List<PredictiveDataViewModel>> GetPredictiveSalesDataAsync();
        Task<DashboardAnalyticsViewModel> GetDashboardSummaryAsync(string period = "7", int? month = null, int? year = null);
        Task<StockLifeCycleViewModel> GetStockLifeCycleAsync(int? productId = null, string? category = null, DateTime? startDate = null, DateTime? endDate = null);
    }
}
