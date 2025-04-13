using System.ComponentModel.DataAnnotations;

namespace Techhive.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage ="Name is required")]
        public String Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public String Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(40,MinimumLength =6, ErrorMessage ="The {0} must be at {2} and max at {1} long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password doesn't Match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "ConfirmPassword is reuired")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

    }
}
