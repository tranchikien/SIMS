using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; } = null!;

        [Required]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; } = null!;

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; } = null!;

        [Required]
        [Display(Name = "Credits")]
        [Range(1, 10, ErrorMessage = "Credits must be between 1 and 10")]
        public int Credits { get; set; }

        [Required]
        public string Status { get; set; } = "Active";
    }
}

