using System;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Models
{
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public string NotificationType { get; set; } = null!; // "GradeAdded", "FacultyAssigned", etc.

        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Message { get; set; } = null!;

        [Required]
        public string RecipientRole { get; set; } = null!; // "Student", "Faculty", "Admin"

        public int? RecipientId { get; set; } // Specific user ID, null if for all users of that role

        public int? RelatedStudentId { get; set; }
        public int? RelatedCourseId { get; set; }
        public int? RelatedGradeId { get; set; }

        public bool IsRead { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

