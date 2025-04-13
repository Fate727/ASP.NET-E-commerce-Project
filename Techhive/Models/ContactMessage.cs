using System.ComponentModel.DataAnnotations;

namespace Techhive.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Message is required.")]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
