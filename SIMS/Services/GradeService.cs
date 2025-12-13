using SIMS.Models;
using SIMS.Repositories;
using System.Linq;
using System.Text.Json;

namespace SIMS.Services
{
    public class GradeService : IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IIdGenerator _idGenerator;
        private readonly IActivityLogService _activityLogService;

        public GradeService(
            IGradeRepository gradeRepository,
            IEnrollmentRepository enrollmentRepository,
            IIdGenerator idGenerator,
            IActivityLogService activityLogService)
        {
            _gradeRepository = gradeRepository;
            _enrollmentRepository = enrollmentRepository;
            _idGenerator = idGenerator;
            _activityLogService = activityLogService;
        }

        public void SaveGrades(GradeViewModel model, int facultyId)
        {
            var enrollments = _enrollmentRepository.GetByCourseId(model.CourseId)
                .Where(e => e.FacultyId == facultyId)
                .ToList();

            foreach (var studentGrade in model.Students)
            {
                if (!studentGrade.FinalScore.HasValue)
                {
                    continue;
                }

                var enrollment = enrollments.FirstOrDefault(e =>
                    e.Id == studentGrade.EnrollmentId &&
                    e.CourseId == model.CourseId);

                if (enrollment == null)
                {
                    continue;
                }

                var studentId = studentGrade.StudentId != 0
                    ? studentGrade.StudentId
                    : enrollment.StudentId;

                // LUÔN xóa tất cả grade cũ của student này trong course này để tránh duplicate
                var existingGrades = _gradeRepository.GetByStudentId(studentId)
                    .Where(g => g.CourseId == model.CourseId)
                    .ToList();
                
                string? oldGradeJson = null;
                if (existingGrades.Any())
                {
                    var oldGrade = existingGrades.First();
                    oldGradeJson = JsonSerializer.Serialize(new
                    {
                        FinalScore = oldGrade.FinalScore,
                        TotalScore = oldGrade.TotalScore,
                        LetterGrade = oldGrade.LetterGrade,
                        Comment = oldGrade.Comment
                    });
                    
                    foreach (var grade in existingGrades)
                    {
                        _gradeRepository.Delete(grade.Id);
                    }
                }

                // Tạo grade mới (luôn tạo mới sau khi xóa để đảm bảo không có duplicate)
                var newGrade = new Grade
                {
                    EnrollmentId = enrollment.Id,
                    StudentId = studentId,
                    CourseId = model.CourseId,
                    FinalScore = studentGrade.FinalScore,
                    TotalScore = studentGrade.FinalScore,
                    LetterGrade = CalculateLetterGrade(studentGrade.FinalScore.Value),
                    Comment = studentGrade.Comment,
                    FacultyId = facultyId // Lưu FacultyId của giảng viên đã chấm điểm
                };
                _gradeRepository.Add(newGrade);

                // Log activity
                var newGradeJson = JsonSerializer.Serialize(new
                {
                    FinalScore = newGrade.FinalScore,
                    TotalScore = newGrade.TotalScore,
                    LetterGrade = newGrade.LetterGrade,
                    Comment = newGrade.Comment
                });

                var activityType = oldGradeJson != null ? "GradeUpdated" : "GradeCreated";
                var description = oldGradeJson != null 
                    ? $"Grade updated for student {studentId} in course {model.CourseId}" 
                    : $"Grade created for student {studentId} in course {model.CourseId}";

                _activityLogService.LogGradeAction(
                    activityType: activityType,
                    gradeId: newGrade.Id,
                    studentId: studentId,
                    courseId: model.CourseId,
                    facultyId: facultyId,
                    description: description,
                    oldValue: oldGradeJson,
                    newValue: newGradeJson,
                    performedBy: $"Faculty ID: {facultyId}"
                );
            }
        }

        public string CalculateLetterGrade(decimal score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            if (score >= 60) return "D";
            return "F";
        }
    }
}
