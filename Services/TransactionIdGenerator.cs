using System.Linq;
using System.Threading.Tasks;
using FoodOrderingSytemAIAnalytics.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public class TransactionIdGenerator : ITransactionIdGenerator
    {
        private readonly ApplicationDbContext _context;
        private static readonly System.Random _random = new System.Random();

        public TransactionIdGenerator(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateNextIdAsync()
        {
            // Format: RCK-JHN-1000-XYZ
            // We search for the MAX receipt number specifically for this prefix
            var lastTransaction = await _context.Transactions
                .Where(t => t.TransactionCode.StartsWith("RCK-JHN-"))
                .OrderByDescending(t => t.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1001;
            if (lastTransaction != null)
            {
                var parts = lastTransaction.TransactionCode.Split('-');
                if (parts.Length >= 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            int salt = _random.Next(100, 999);
            return $"RCK-JHN-{nextNumber}-{salt}";
        }
    }
}
