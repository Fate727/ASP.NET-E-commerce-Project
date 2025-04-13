using Techhive.Models;

namespace Techhive.HomeModel
{
    public class RelatedProductViewModel
    {
        public Product Product { get; set; }
        public List<Product> RelatedProducts { get; set; }
    }
}
