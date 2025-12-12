using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class StudentProfileViewModel
    {
        // Read-only fields
        public int Id { get; set; }
        
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = string.Empty;

        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Program")]
        public string Program { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Gender")]
        public string? Gender { get; set; }

        // Editable fields
        [Display(Name = "Phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Display(Name = "Address")]
        [StringLength(255)]
        public string? Address { get; set; }
    }
}

