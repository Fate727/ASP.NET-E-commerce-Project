using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Techhive.Models
{
    public class Product
    {
        [Key]
        public int P_Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public string Detail { get; set; }

        [Required]
        public string Image1 { get; set; }
        public string Image2 { get; set; }
        public string Image3 { get; set; }

        [Required]
        public int B_Id { get; set; }
        [ForeignKey("B_Id")]
        [ValidateNever]
        public Brand Brands { get; set; }

        // Extra Date Fields
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
