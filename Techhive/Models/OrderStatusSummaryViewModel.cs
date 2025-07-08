namespace Techhive.Models
{
    public class OrderStatusSummaryViewModel
    {
        public int TotalOrderCount { get; set; }
        public decimal TotalOrderAmount { get; set; }

        public int CompleteOrderCount { get; set; }
        public decimal CompleteOrderAmount { get; set; }

        public int PendingOrderCount { get; set; }
        public decimal PendingOrderAmount { get; set; }

        public int CancelOrderCount { get; set; }
        public decimal CancelOrderAmount { get; set; }

    }
}
