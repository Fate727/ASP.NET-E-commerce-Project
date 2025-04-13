using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Techhive.Models
{
    public class ProductViewModel
    {
        public int P_Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public string Detail { get; set; }

        // Removed [Required] attribute from image fields
        [ValidateNever]
        public IFormFile? Image1 { get; set; }

        [ValidateNever]
        public IFormFile? Image2 { get; set; }

        [ValidateNever]
        public IFormFile? Image3 { get; set; }

        [Required]
        public int B_Id { get; set; }

        [ForeignKey("B_Id")]
        [ValidateNever]
        public IEnumerable<SelectListItem> BrandList { get; set; }

        public string? ExistingImage1 { get; set; }
        public string? ExistingImage2 { get; set; }
        public string? ExistingImage3 { get; set; }
    }
}
