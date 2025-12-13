using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface IActivityLogRepository
    {
        IEnumerable<ActivityLog> GetAll();
        ActivityLog? GetById(int id);
        IEnumerable<ActivityLog> GetByStudentId(int studentId);
        IEnumerable<ActivityLog> GetByCourseId(int courseId);
        IEnumerable<ActivityLog> GetByFacultyId(int facultyId);
        IEnumerable<ActivityLog> GetByActivityType(string activityType);
        void Add(ActivityLog activityLog);
    }
}
