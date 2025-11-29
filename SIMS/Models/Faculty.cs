using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Faculty
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Faculty ID")]
        public string FacultyId { get; set; } = null!;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Department")]
        public string Department { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Active";
    }
}

