using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSytemAIAnalytics.Models
{
    [Index(nameof(TransactionCode), IsUnique = true)]
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TransactionCode { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CashReceived { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Change { get; set; }

        public bool IsWeekend { get; set; } // Feature for Prophet (Seasonality)

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation Property
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();
    }
}
