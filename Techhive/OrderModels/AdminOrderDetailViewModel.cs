using Techhive.Models;

namespace Techhive.OrderModels
{
    public class AdminOrderDetailViewModel
    {
    public List<Order> Orders { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    public List<Product> Products { get; set; }

    public decimal TotalAmount { get; set; }
    public int TotalOrders { get; set; }
    public string Status { get; set; }
    }
}
