using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Techhive.Models;

namespace Techhive.OrderModels
{
    public class Order
    {
      
            [Key]
            public int OrderId { get; set; }

            [Required]
            public string UserId { get; set; }

            [ForeignKey("UserId")]
            public Users User { get; set; }

            public DateTime OrderDate { get; set; } = DateTime.Now;
            public DateTime DeliveredDate { get; set; } 

            [Required]
            public decimal TotalAmount { get; set; }

            [Required]
            public string Status { get; set; } = "Pending";

            [Required]
            public string ShippingAddress { get; set; }

            [Required]
            public string PaymentMethod { get; set; }

            public ICollection<OrderItem> OrderItems { get; set; }

    }
}
