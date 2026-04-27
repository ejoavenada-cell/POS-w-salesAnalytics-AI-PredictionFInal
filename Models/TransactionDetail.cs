using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodOrderingSytemAIAnalytics.Models
{
    public class TransactionDetail
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TransactionId { get; set; }

        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; } = null!;

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [Required]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Price at time of transaction

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }
    }
}
