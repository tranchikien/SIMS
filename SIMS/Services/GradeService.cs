using SIMS.Models;
using SIMS.Repositories;
using System.Linq;

namespace SIMS.Services
{
    public class GradeService : IGradeService
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IIdGenerator _idGenerator;

        public GradeService(
            IGradeRepository gradeRepository,
            IEnrollmentRepository enrollmentRepository,
            IIdGenerator idGenerator)
        {
            _gradeRepository = gradeRepository;
            _enrollmentRepository = enrollmentRepository;
            _idGenerator = idGenerator;
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
                
                foreach (var oldGrade in existingGrades)
                {
                    _gradeRepository.Delete(oldGrade.Id);
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
