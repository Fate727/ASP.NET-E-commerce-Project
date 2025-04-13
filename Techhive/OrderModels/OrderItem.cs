using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Techhive.Models;

namespace Techhive.OrderModels
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        public int P_Id { get; set; }  // Match Product primary key

        [ForeignKey("P_Id")]
        public Product Products { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        [NotMapped]
        public decimal Total => Quantity * Price;

    }
}
