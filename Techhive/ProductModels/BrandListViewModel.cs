using Techhive.Models;

namespace Techhive.ProductModels
{
    public class BrandListViewModel
    {
        public IEnumerable<Brand> Brands { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchName { get; set; }
    }
}
