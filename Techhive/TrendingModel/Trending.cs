using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Techhive.Models;

namespace Techhive.TrendingModel
{
    public class Trending
    {
        [Key]
        public int T_id { get; set; }

        public int P_Id { get; set; }
        public Product Product { get; set; } // 🔥 Needed for FK

        public double TrendingScore { get; set; }
        public int OrderCount { get; set; }
        public int TotalViews { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
