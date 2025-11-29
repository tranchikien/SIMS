using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class StudentEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Student ID is required")]
        [Display(Name = "Student ID")]
        public string StudentId { get; set; } = null!;

        [Required(ErrorMessage = "Program / Major is required")]
        [Display(Name = "Program / Major")]
        public string Program { get; set; } = null!;

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string? ConfirmPassword { get; set; }

        [Required]
        public string Role { get; set; } = "Student";

        [Required]
        public string Status { get; set; } = "Active";
    }
}

