using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSytemAIAnalytics.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Icon { get; set; } = "fas fa-utensils"; // Default icon

        public bool IsActive { get; set; } = true;
    }
}
