using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = null!;

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Display(Name = "Program / Major")]
        public string Program { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Active";
    }
}

