using Techhive.ProductModels;

namespace Techhive.OrderModels
{
    using System.ComponentModel.DataAnnotations;

    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Payment Method is required.")]
        public string PaymentMethod { get; set; }

        public List<Cart> CartItems { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
    }

}
