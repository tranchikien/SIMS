using Microsoft.AspNetCore.Mvc;

namespace SIMS.Services
{
    /// <summary>
    /// Service for handling authorization logic (SOLID: Single Responsibility)
    /// </summary>
    public interface IAuthorizationService
    {
        /// <summary>
        /// Checks if current user has the specified role
        /// </summary>
        bool HasRole(HttpContext context, string role);
        
        /// <summary>
        /// Gets the current user's role from session
        /// </summary>
        string? GetCurrentRole(HttpContext context);
        
        /// <summary>
        /// Gets the current user's ID from session
        /// </summary>
        int? GetCurrentUserId(HttpContext context);
        
        /// <summary>
        /// Ensures user has admin role, otherwise redirects appropriately
        /// </summary>
        IActionResult EnsureAdmin(HttpContext context, Func<IActionResult> onSuccess);
        
        /// <summary>
        /// Ensures user has student role, otherwise redirects appropriately
        /// </summary>
        IActionResult EnsureStudent(HttpContext context, Func<IActionResult> onSuccess);
        
        /// <summary>
        /// Ensures user has faculty role, otherwise redirects appropriately
        /// </summary>
        IActionResult EnsureFaculty(HttpContext context, Func<IActionResult> onSuccess);
    }
}

