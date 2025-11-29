using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class EnrollmentViewModel
    {
        [Required(ErrorMessage = "Please select a student")]
        [Display(Name = "Student")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "Please select a course")]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

        [Display(Name = "Faculty (Optional)")]
        public int? FacultyId { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Enrolled";
    }
}

