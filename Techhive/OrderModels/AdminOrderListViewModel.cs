namespace Techhive.OrderModels
{
    public class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; }

        public string SearchTerm { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public int PageSize { get; set; } = 10;
    }
}
