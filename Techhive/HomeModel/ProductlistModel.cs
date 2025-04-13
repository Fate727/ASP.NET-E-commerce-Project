using Techhive.Models;

namespace Techhive.HomeModel
{
    public class ProductViewModel
    {
        public List<Brand> Brands { get; set; }
        public List<Product> LatestProducts { get; set; }
        public List<Product> FeaturedProducts { get; set; }
        public List<Product> RecommendProducts { get; set; }
    }
}
