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

        [Display(Name = "Phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Address")]
        [StringLength(255)]
        public string? Address { get; set; }

        [Display(Name = "Gender")]
        [StringLength(10)]
        public string? Gender { get; set; }
    }
}

