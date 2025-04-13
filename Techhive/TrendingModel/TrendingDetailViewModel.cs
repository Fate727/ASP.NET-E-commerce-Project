using Techhive.Models;

namespace Techhive.TrendingModel
{
    public class TrendingDetailViewModel
    {
        public Product Product { get; set; }
        public List<Recommendation> Recommendations { get; set; }
        public List<Users> User { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; }
    }
}
