namespace FoodOrderingSytemAIAnalytics.Services
{
    public interface IPricingService
    {
        decimal CalculateFinalPrice(decimal originalPrice, decimal discountPercent);
        bool HasActiveDiscount(decimal discountPercent);
    }
}
