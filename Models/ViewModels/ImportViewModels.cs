using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class ProductImportRow
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal Stock { get; set; }
        public string Category { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal DiscountPercent { get; set; } = 0;
    }

    public class ImportResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ImportedCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
