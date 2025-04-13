using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Techhive.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage ="Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "The {0} must be at {2} and max at {1} long.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name ="Remeber me?")]
        public bool RememberMe { get; set; }
    }
}
