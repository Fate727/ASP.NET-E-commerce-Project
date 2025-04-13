using System.Collections.Generic;

namespace Techhive.Models
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; }
        public string SearchQuery { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
