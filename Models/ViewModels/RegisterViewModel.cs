using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string Username { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Age must be between 1 and 120")]
        public int? Age { get; set; }

        [StringLength(10)]
        public string Sex { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("Admin|Staff", ErrorMessage = "Role must be 'Admin' or 'Staff'")]
        public string Role { get; set; } = "Staff";

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
