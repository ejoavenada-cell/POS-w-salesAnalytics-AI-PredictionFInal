using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetAllTransactionsAsync();
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end);
        Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(int userId);
        Task<SalesReportViewModel> GetSalesReportAsync(DateTime start, DateTime end);
        Task<IEnumerable<Transaction>> SearchTransactionsAsync(string searchTerm);
        Task<decimal> GetDailyRevenueAsync(DateTime date);
    }
}
