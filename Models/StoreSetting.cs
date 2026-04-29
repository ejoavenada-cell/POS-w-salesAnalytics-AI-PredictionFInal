using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSytemAIAnalytics.Models
{
    public class StoreSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string StoreName { get; set; } = "Tasty Station";

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Microsoft.AspNetCore.Http.IFormFile? LogoFile { get; set; }

        public string CurrencySymbol { get; set; } = "₱";
        public string PrimaryColor { get; set; } = "#2D9F96";
        
        // AI Tuning Parameters
        public int AIForecastHorizonHours { get; set; } = 720; // Default 30 days
        public double AISensitivity { get; set; } = 0.1; // changepoint_prior_scale
        public string AISeasonalityMode { get; set; } = "multiplicative";
        public DateTime? LastAISync { get; set; }
    }
}
