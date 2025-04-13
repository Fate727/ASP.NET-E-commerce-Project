using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Techhive.Models;

namespace Techhive.TrendingModel
{
    public class Recommendation
    {
        [Key]
        public int R_id { get; set; }

        public string UserID { get; set; }
        public Users User { get; set; } // 🔥 Needed for FK

        public int P_Id { get; set; }
        public Product Product { get; set; } // 🔥 Needed for FK

        public int V_Count { get; set; }
        public string Type { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsOrdered { get; set; }
    }

}
