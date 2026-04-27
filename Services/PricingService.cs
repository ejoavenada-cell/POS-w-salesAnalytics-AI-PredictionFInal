namespace FoodOrderingSytemAIAnalytics.Services
{
    public class PricingService : IPricingService
    {
        public decimal CalculateFinalPrice(decimal originalPrice, decimal discountPercent)
        {
            if (discountPercent <= 0) return originalPrice;
            if (discountPercent > 100) return 0;

            return originalPrice * (1 - (discountPercent / 100));
        }

        public bool HasActiveDiscount(decimal discountPercent)
        {
            return discountPercent > 0;
        }
    }
}
