using System;

namespace SIMS.Models
{
    public class ActivityLogViewModel
    {
        public int Id { get; set; }
        public string ActivityType { get; set; } = null!;
        public int? GradeId { get; set; }
        public int? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? StudentCode { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public int? FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public string? FacultyCode { get; set; }
        public string Description { get; set; } = null!;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PerformedBy { get; set; } = null!;
    }
}

