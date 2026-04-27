using System.Linq;
using System.Threading.Tasks;
using FoodOrderingSytemAIAnalytics.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public class TransactionIdGenerator : ITransactionIdGenerator
    {
        private readonly ApplicationDbContext _context;

        public TransactionIdGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateNextIdAsync()
        {
            // Format: RCK-JHN-1000+
            var lastTransaction = await _context.Transactions
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1001;
            if (lastTransaction != null && lastTransaction.TransactionCode.StartsWith("RCK-JHN-"))
            {
                var parts = lastTransaction.TransactionCode.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"RCK-JHN-{nextNumber}";
        }
    }
}
