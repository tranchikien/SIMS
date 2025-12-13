using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Services
{
    public interface IActivityLogService
    {
        void LogGradeAction(string activityType, int? gradeId, int? studentId, int? courseId, int? facultyId, string description, string? oldValue, string? newValue, string performedBy);
        IEnumerable<ActivityLog> GetActivityLogs(int? studentId = null, int? courseId = null, int? facultyId = null, string? activityType = null);
        IEnumerable<ActivityLog> GetGradeHistory(int studentId, int? courseId = null);
        IEnumerable<ActivityLogViewModel> GetActivityLogsWithDetails(int? studentId = null, int? courseId = null, int? facultyId = null, string? activityType = null);
    }
}

