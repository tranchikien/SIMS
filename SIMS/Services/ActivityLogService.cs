using SIMS.Models;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace SIMS.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IFacultyRepository _facultyRepository;
        private readonly ICourseRepository _courseRepository;

        public ActivityLogService(
            IActivityLogRepository activityLogRepository,
            IStudentRepository studentRepository,
            IFacultyRepository facultyRepository,
            ICourseRepository courseRepository)
        {
            _activityLogRepository = activityLogRepository;
            _studentRepository = studentRepository;
            _facultyRepository = facultyRepository;
            _courseRepository = courseRepository;
        }

        public void LogGradeAction(string activityType, int? gradeId, int? studentId, int? courseId, int? facultyId, string description, string? oldValue, string? newValue, string performedBy)
        {
            var activityLog = new ActivityLog
            {
                ActivityType = activityType,
                GradeId = gradeId,
                StudentId = studentId,
                CourseId = courseId,
                FacultyId = facultyId,
                Description = description,
                OldValue = oldValue,
                NewValue = newValue,
                PerformedBy = performedBy,
                CreatedAt = System.DateTime.Now
            };

            _activityLogRepository.Add(activityLog);
        }

        public IEnumerable<ActivityLog> GetActivityLogs(int? studentId = null, int? courseId = null, int? facultyId = null, string? activityType = null)
        {
            var logs = _activityLogRepository.GetAll();

            if (studentId.HasValue)
            {
                logs = logs.Where(l => l.StudentId == studentId.Value);
            }

            if (courseId.HasValue)
            {
                logs = logs.Where(l => l.CourseId == courseId.Value);
            }

            if (facultyId.HasValue)
            {
                logs = logs.Where(l => l.FacultyId == facultyId.Value);
            }

            if (!string.IsNullOrEmpty(activityType))
            {
                logs = logs.Where(l => l.ActivityType == activityType);
            }

            return logs;
        }

        public IEnumerable<ActivityLog> GetGradeHistory(int studentId, int? courseId = null)
        {
            var logs = _activityLogRepository.GetByStudentId(studentId)
                .Where(l => l.ActivityType.StartsWith("Grade"));

            if (courseId.HasValue)
            {
                logs = logs.Where(l => l.CourseId == courseId.Value);
            }

            return logs;
        }

        public IEnumerable<ActivityLogViewModel> GetActivityLogsWithDetails(int? studentId = null, int? courseId = null, int? facultyId = null, string? activityType = null)
        {
            var logs = GetActivityLogs(studentId, courseId, facultyId, activityType).ToList();
            var students = _studentRepository.GetAll().ToList();
            var faculties = _facultyRepository.GetAll().ToList();
            var courses = _courseRepository.GetAll().ToList();

            return logs.Select(log => new ActivityLogViewModel
            {
                Id = log.Id,
                ActivityType = log.ActivityType,
                GradeId = log.GradeId,
                StudentId = log.StudentId,
                StudentName = log.StudentId.HasValue ? students.FirstOrDefault(s => s.Id == log.StudentId.Value)?.FullName : null,
                StudentCode = log.StudentId.HasValue ? students.FirstOrDefault(s => s.Id == log.StudentId.Value)?.StudentId : null,
                CourseId = log.CourseId,
                CourseName = log.CourseId.HasValue ? courses.FirstOrDefault(c => c.Id == log.CourseId.Value)?.CourseName : null,
                CourseCode = log.CourseId.HasValue ? courses.FirstOrDefault(c => c.Id == log.CourseId.Value)?.CourseCode : null,
                FacultyId = log.FacultyId,
                FacultyName = log.FacultyId.HasValue ? faculties.FirstOrDefault(f => f.Id == log.FacultyId.Value)?.FullName : null,
                FacultyCode = log.FacultyId.HasValue ? faculties.FirstOrDefault(f => f.Id == log.FacultyId.Value)?.FacultyId : null,
                Description = log.Description,
                OldValue = log.OldValue,
                NewValue = log.NewValue,
                CreatedAt = log.CreatedAt,
                PerformedBy = log.PerformedBy
            });
        }
    }
}
