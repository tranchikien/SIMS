namespace SIMS.Models
{
    /// <summary>
    /// ViewModel for displaying enrollment with related data
    /// </summary>
    public class EnrollmentDisplayViewModel
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int? FacultyId { get; set; }
        public string Status { get; set; } = "Enrolled";
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string FacultyName { get; set; } = string.Empty;
    }
}

