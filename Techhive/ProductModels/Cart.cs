using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Techhive.Models;

namespace Techhive.ProductModels
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign key to AspNetUsers table

        [ForeignKey("UserId")]
        public Users User { get; set; }

        [Required]
        public int P_Id { get; set; } // Foreign key to Product table

        [ForeignKey("P_Id")]
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
