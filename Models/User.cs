using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FoodOrderingSytemAIAnalytics.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; } = string.Empty;

        [Range(18, 100)]
        public int? Age { get; set; }

        [StringLength(10)]
        public string Sex { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // Admin / Staff

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsApproved { get; set; } = false;

        public DateTime DateCreated { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public Microsoft.AspNetCore.Http.IFormFile? ProfileImage { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Navigation Property
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
