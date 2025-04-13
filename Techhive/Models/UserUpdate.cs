using System.ComponentModel.DataAnnotations;

namespace Techhive.Models
{
    public class UserUpdate
    {
        [Required(ErrorMessage = "Name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        public string PhoneNumber { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Old password is required")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [StringLength(40, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} and maximum {1} characters long.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm new password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password doesn't match.")]
        public string ConfirmNewPassword { get; set; }
    }
}