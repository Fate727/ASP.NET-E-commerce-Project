using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Techhive.Models
{
    public class Brand
    {
        [Key]
        public int B_Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Image { get; set; } // Store the image path or filename

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
