using Techhive.Models;

namespace Techhive.OrderModels
{
    public class OrderDetialModel
    {
        public List<Order> Orders { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<Product> Products { get; set; }
    }
}
