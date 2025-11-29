using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public string Status { get; set; } = "Enrolled"; // Enrolled, Completed, Dropped

        public int? FacultyId { get; set; } // Optional: assigned faculty
    }
}

