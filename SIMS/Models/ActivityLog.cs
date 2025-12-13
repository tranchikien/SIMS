using System;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [Required]
        public string ActivityType { get; set; } = null!; // GradeCreated, GradeUpdated, GradeDeleted

        public int? GradeId { get; set; }
        public int? StudentId { get; set; }
        public int? CourseId { get; set; }
        public int? FacultyId { get; set; } // Faculty who performed the action

        [Required]
        public string Description { get; set; } = null!; // Description of the action
        public string? OldValue { get; set; } // Old grade value (JSON)
        public string? NewValue { get; set; } // New grade value (JSON)

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public string PerformedBy { get; set; } = null!; // Username or name of person who performed action
    }
}
