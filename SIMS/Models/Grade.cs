using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Grade
    {
        public int Id { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        public decimal? FinalScore { get; set; }

        [Range(0, 100, ErrorMessage = "Total score must be between 0 and 100")]
        public decimal? TotalScore { get; set; }

        public string? LetterGrade { get; set; } // A, B, C, D, F

        public string? Comment { get; set; } // Optional comment from faculty

        public int? FacultyId { get; set; } // ID của giảng viên đã chấm điểm
    }
}

