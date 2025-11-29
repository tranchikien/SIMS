using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = null!;

        [Required]
        [StringLength(255)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = null!; // Admin, Faculty, Student

        public int? ReferenceId { get; set; } // NULL cho Admin, ID từ Students/Faculty cho các role khác

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // Active, Inactive
    }
}

