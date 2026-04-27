using FoodOrderingSytemAIAnalytics.Data;
using FoodOrderingSytemAIAnalytics.Models;
using FoodOrderingSytemAIAnalytics.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ApplicationDbContext _context;

        public TransactionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.TransactionDetails)
                    .ThenInclude(td => td.Product)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.TransactionDetails)
                    .ThenInclude(td => td.Product)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Where(t => t.Date >= start && t.Date <= end)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(int userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<SalesReportViewModel> GetSalesReportAsync(DateTime start, DateTime end)
        {
            var transactions = await GetTransactionsByDateRangeAsync(start, end);
            
            return new SalesReportViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = transactions.Sum(t => t.TotalAmount),
                TotalTransactions = transactions.Count()
            };
        }

        public async Task<IEnumerable<Transaction>> SearchTransactionsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return await GetAllTransactionsAsync();

            return await _context.Transactions
                .Include(t => t.User)
                .Where(t => t.TransactionCode.Contains(searchTerm))
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<decimal> GetDailyRevenueAsync(DateTime date)
        {
            return await _context.Transactions
                .Where(t => t.Date.Date == date.Date)
                .SumAsync(t => t.TotalAmount);
        }
    }
}
