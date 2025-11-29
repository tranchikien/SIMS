using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace SIMS.Services
{
    /// <summary>
    /// Implementation of authorization service (SOLID: Single Responsibility)
    /// </summary>
    public class AuthorizationService : IAuthorizationService
    {
        public bool HasRole(HttpContext context, string role)
        {
            var currentRole = context.Session.GetString("Role");
            return !string.IsNullOrEmpty(currentRole) && currentRole == role;
        }

        public string? GetCurrentRole(HttpContext context)
        {
            return context.Session.GetString("Role");
        }

        public int? GetCurrentUserId(HttpContext context)
        {
            var userIdString = context.Session.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }

        public IActionResult EnsureAdmin(HttpContext context, Func<IActionResult> onSuccess)
        {
            var role = GetCurrentRole(context);
            
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectBasedOnRole(role);
            }

            return onSuccess();
        }

        public IActionResult EnsureStudent(HttpContext context, Func<IActionResult> onSuccess)
        {
            var role = GetCurrentRole(context);
            
            if (string.IsNullOrEmpty(role) || role != "Student")
            {
                return RedirectBasedOnRole(role);
            }

            return onSuccess();
        }

        public IActionResult EnsureFaculty(HttpContext context, Func<IActionResult> onSuccess)
        {
            var role = GetCurrentRole(context);
            
            if (string.IsNullOrEmpty(role) || role != "Faculty")
            {
                return RedirectBasedOnRole(role);
            }

            return onSuccess();
        }

        private RedirectToActionResult RedirectBasedOnRole(string? role)
        {
            return role switch
            {
                "Student" => new RedirectToActionResult("Index", "StudentDashboard", null),
                "Faculty" => new RedirectToActionResult("Index", "FacultyDashboard", null),
                "Admin" => new RedirectToActionResult("Index", "Dashboard", null),
                _ => new RedirectToActionResult("Index", "Login", null)
            };
        }
    }
}

