using System.Threading.Tasks;

namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface ITransactionIdGenerator
    {
        Task<string> GenerateNextIdAsync();
    }
}
