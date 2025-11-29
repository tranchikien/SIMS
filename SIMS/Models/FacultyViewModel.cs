using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class FacultyViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Faculty ID is required")]
        [Display(Name = "Faculty ID")]
        public string FacultyId { get; set; } = null!;

        [Required(ErrorMessage = "Department is required")]
        [Display(Name = "Department")]
        public string Department { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Please confirm your password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = null!;

        [Required]
        public string Role { get; set; } = "Faculty";
    }
}

