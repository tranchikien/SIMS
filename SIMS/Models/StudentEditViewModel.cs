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

        [Display(Name = "Phone")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string? Phone { get; set; }

        [Display(Name = "Address")]
        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; }

        [Display(Name = "Gender")]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters.")]
        public string? Gender { get; set; }
    }
}

