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
    }
}
