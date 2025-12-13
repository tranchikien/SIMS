using SIMS.Data;
using SIMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly SIMSDbContext _context;

        public ActivityLogRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public IEnumerable<ActivityLog> GetAll()
        {
            return _context.ActivityLogs.OrderByDescending(a => a.CreatedAt).ToList();
        }

        public ActivityLog? GetById(int id)
        {
            return _context.ActivityLogs.FirstOrDefault(a => a.Id == id);
        }

        public IEnumerable<ActivityLog> GetByStudentId(int studentId)
        {
            return _context.ActivityLogs
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        public IEnumerable<ActivityLog> GetByCourseId(int courseId)
        {
            return _context.ActivityLogs
                .Where(a => a.CourseId == courseId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        public IEnumerable<ActivityLog> GetByFacultyId(int facultyId)
        {
            return _context.ActivityLogs
                .Where(a => a.FacultyId == facultyId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        public IEnumerable<ActivityLog> GetByActivityType(string activityType)
        {
            return _context.ActivityLogs
                .Where(a => a.ActivityType == activityType)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        public void Add(ActivityLog activityLog)
        {
            _context.ActivityLogs.Add(activityLog);
            _context.SaveChanges();
        }
    }
}
