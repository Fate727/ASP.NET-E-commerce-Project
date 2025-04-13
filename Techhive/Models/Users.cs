using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Techhive.Models
{
    public class Users : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
    }
}
