using SIMS.Models;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Services
{
    public class StudentDashboardService : IStudentDashboardService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IGradeRepository _gradeRepository;

        public StudentDashboardService(
            IStudentRepository studentRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IFacultyRepository facultyRepository,
            IGradeRepository gradeRepository)
        {
            _studentRepository = studentRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _facultyRepository = facultyRepository;
            _gradeRepository = gradeRepository;
        }

        public StudentDashboardInfo GetDashboardInfo(int studentId)
        {
            var student = _studentRepository.GetById(studentId);
            if (student == null)
            {
                return new StudentDashboardInfo();
            }

            var enrollments = _enrollmentRepository.GetByStudentId(studentId)
                .Where(e => e.Status == "Enrolled")
                .ToList();

            var courses = _courseRepository.GetAll().ToList();
            var faculties = _facultyRepository.GetAll().ToList();

            var enrolledCourses = enrollments
                .Select(e => new
                {
                    Enrollment = e,
                    Course = courses.FirstOrDefault(c => c.Id == e.CourseId),
                    Faculty = faculties.FirstOrDefault(f => f.Id == e.FacultyId)
                })
                .Where(x => x.Course != null)
                .Select(x => new
                {
                    CourseCode = x.Course!.CourseCode,
                    CourseName = x.Course.CourseName,
                    FacultyName = x.Faculty?.FullName ?? "Not Assigned",
                    Credits = x.Course.Credits,
                    Status = x.Enrollment.Status
                })
                .ToList();

            var totalCredits = enrolledCourses.Sum(c => c.Credits);

            var gpa = CalculateGPA(studentId, enrollments, courses);

            return new StudentDashboardInfo
            {
                Student = student,
                EnrolledCourses = enrolledCourses,
                TotalCredits = totalCredits,
                GPA = gpa
            };
        }

        public IEnumerable<dynamic> GetEnrolledCourses(int studentId)
        {
            var enrollments = _enrollmentRepository.GetByStudentId(studentId).ToList();
            var courses = _courseRepository.GetAll().ToList();
            var faculties = _facultyRepository.GetAll().ToList();

            return enrollments
                .Select(e => new
                {
                    Enrollment = e,
                    Course = courses.FirstOrDefault(c => c.Id == e.CourseId),
                    Faculty = faculties.FirstOrDefault(f => f.Id == e.FacultyId)
                })
                .Where(x => x.Course != null)
                .Select(x => new
                {
                    CourseCode = x.Course!.CourseCode,
                    CourseName = x.Course.CourseName,
                    FacultyName = x.Faculty?.FullName ?? "Not Assigned",
                    Credits = x.Course.Credits,
                    Status = x.Enrollment.Status
                })
                .ToList();
        }

        public IEnumerable<StudentGradeDisplay> GetGrades(int studentId)
        {
            var grades = _gradeRepository.GetByStudentId(studentId).ToList();
            var courses = _courseRepository.GetAll().ToList();

            return grades
                .Select(g => new
                {
                    Grade = g,
                    Course = courses.FirstOrDefault(c => c.Id == g.CourseId)
                })
                .Where(x => x.Course != null)
                .Select(x => new StudentGradeDisplay
                {
                    CourseName = x.Course!.CourseName,
<<<<<<< HEAD
                    FinalScore = x.Grade.FinalScore,
=======
                    MidtermScore = x.Grade.MidtermScore,
                    FinalScore = x.Grade.FinalScore,
                    AssignmentScore = x.Grade.AssignmentScore,
>>>>>>> ee194c07c122bf48106af85d3475a24fce023e6c
                    TotalScore = x.Grade.TotalScore,
                    LetterGrade = !string.IsNullOrEmpty(x.Grade.LetterGrade)
                        ? x.Grade.LetterGrade
                        : (x.Grade.TotalScore.HasValue ? GetLetterGrade(x.Grade.TotalScore.Value) : null),
                    Comment = x.Grade.Comment
                })
                .ToList();
        }

        private decimal? CalculateGPA(int studentId, List<Enrollment> enrollments, List<Course> courses)
        {
            var grades = _gradeRepository.GetByStudentId(studentId)
                .Where(g => g.TotalScore.HasValue)
                .ToList();

            if (!grades.Any())
            {
                return null;
            }

            var gradePoints = new Dictionary<string, decimal>
            {
                { "A", 4.0m },
                { "B", 3.0m },
                { "C", 2.0m },
                { "D", 1.0m },
                { "F", 0.0m }
            };

            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();
            var gradedEnrollments = grades
                .Where(g => enrolledCourseIds.Contains(g.CourseId))
                .ToList();

            if (!gradedEnrollments.Any())
            {
                return null;
            }

            var totalPoints = 0.0m;
            var totalCreditsForGPA = 0;

            foreach (var grade in gradedEnrollments)
            {
                var enrollment = enrollments.FirstOrDefault(e => e.CourseId == grade.CourseId);
                if (enrollment != null && enrollment.Status == "Enrolled")
                {
                    var course = courses.FirstOrDefault(c => c.Id == grade.CourseId);
                    if (course != null)
                    {
                        var letter = grade.LetterGrade;
                        if (string.IsNullOrEmpty(letter) && grade.TotalScore.HasValue)
                        {
                            letter = GetLetterGrade(grade.TotalScore.Value);
                        }

                        if (!string.IsNullOrEmpty(letter) && gradePoints.ContainsKey(letter))
                        {
                            totalPoints += gradePoints[letter] * course.Credits;
                            totalCreditsForGPA += course.Credits;
                        }
                    }
                }
            }

            return totalCreditsForGPA > 0 ? totalPoints / totalCreditsForGPA : null;
        }

        private string GetLetterGrade(decimal score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            if (score >= 60) return "D";
            return "F";
        }
    }
}

