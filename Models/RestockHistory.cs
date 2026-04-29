using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSytemAIAnalytics.Models
{
    public class RestockHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal QuantityRestocked { get; set; }

        [Required]
        public DateTime RestockDate { get; set; }

        public string? Remarks { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal StockAfterRestock { get; set; }
    }
}
