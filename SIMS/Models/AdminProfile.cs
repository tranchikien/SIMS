using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class AdminProfile
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "System Administrator";
    }
}

