using Techhive.Models;

namespace Techhive.HomeModel
{
    public class ShopViewModel
    {
        public List<Product> Products { get; set; }
        public List<Brand> Brands { get; set; }
        public string SearchQuery { get; set; }
        public List<int> BrandIds { get; set; }
        public string PriceRange { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int TotalProducts { get; set; }
        public int CurrentPage { get; set; } // Current page
        public int TotalPages { get; set; } // Total pages
    }



}
