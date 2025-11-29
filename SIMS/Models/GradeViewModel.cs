using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SIMS.Models
{
    public class GradeViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = null!;
        public List<StudentGradeItem> Students { get; set; } = new List<StudentGradeItem>();
    }

    public class StudentGradeItem
    {
        public int EnrollmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentIdCode { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        
        [Range(0, 100, ErrorMessage = "Final Score must be between 0 and 100")]
        [Display(Name = "Final Score")]
        public decimal? FinalScore { get; set; }
        
        [Display(Name = "Comment")]
        public string? Comment { get; set; }
    }
}

