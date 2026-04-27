using System.Collections.Generic;
using FoodOrderingSytemAIAnalytics.Models;

namespace FoodOrderingSytemAIAnalytics.Models.ViewModels
{
    public class POSTerminalViewModel
    {
        public IEnumerable<ProductDisplayViewModel> Products { get; set; } = new List<ProductDisplayViewModel>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public CartViewModel Cart { get; set; } = new CartViewModel();
        public string? SelectedCategory { get; set; }
    }
}
