using System.ComponentModel.DataAnnotations;

namespace Techhive.Models
{
    public class ChangePasswordViewModelcs
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public String Email { get; set; }

        [Required(ErrorMessage = "NewPassword is required")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "The {0} must be at {2} and max at {1} long.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password doesn't Match.")]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "ConfirmNewPassword is reuired")]
        [DataType(DataType.Password)]
        [Display(Name ="Confirm New Password")]
        public string ConfirmNewPassword { get; set; }
    }
}
