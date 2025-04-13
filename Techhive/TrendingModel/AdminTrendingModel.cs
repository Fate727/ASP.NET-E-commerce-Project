using Techhive.Models;

namespace Techhive.TrendingModel
{
    public class AdminTrendingModel
    {
        public List<Trending> Trend { get; set; }
        public List<Product> Products { get; set; }

        // Pagination properties
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        // Search query property
        public string SearchQuery { get; set; }
    }

}
