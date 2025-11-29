using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Services
{
    public interface IStudentDashboardService
    {
        StudentDashboardInfo GetDashboardInfo(int studentId);
        IEnumerable<StudentGradeDisplay> GetGrades(int studentId);
        IEnumerable<dynamic> GetEnrolledCourses(int studentId);
    }

    public class StudentDashboardInfo
    {
        public Student Student { get; set; } = null!;
        public IEnumerable<dynamic> EnrolledCourses { get; set; } = new List<dynamic>();
        public int TotalCredits { get; set; }
        public decimal? GPA { get; set; }
    }
}

