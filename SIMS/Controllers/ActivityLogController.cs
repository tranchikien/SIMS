using Microsoft.AspNetCore.Mvc;
using SIMS.Services;
using System.Linq;

namespace SIMS.Controllers
{
    public class ActivityLogController : Controller
    {
        private readonly IActivityLogService _activityLogService;
        private readonly IAuthorizationService _authorizationService;

        public ActivityLogController(
            IActivityLogService activityLogService,
            IAuthorizationService authorizationService)
        {
            _activityLogService = activityLogService;
            _authorizationService = authorizationService;
        }

        // GET: ActivityLog
        public IActionResult Index(int? studentId, int? courseId, int? facultyId, string? activityType)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Activity Log";
                ViewData["breadcrumb"] = "Activity Log";
                ViewData["breadcrumb-item"] = "activity-log";

                var logs = _activityLogService.GetActivityLogsWithDetails(studentId, courseId, facultyId, activityType);
                return View(logs.ToList());
            });
        }
    }
}

