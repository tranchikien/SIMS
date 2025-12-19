using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace SIMS.Controllers
{
    /// <summary>
    /// Controller for admin dashboard (SOLID: Single Responsibility - Only handles HTTP requests/responses)
    /// </summary>
    public class DashboardController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IFacultyRepository _facultyRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IAdminProfileRepository _adminProfileRepository;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public DashboardController(
            IStudentRepository studentRepository,
            IFacultyRepository facultyRepository,
            ICourseRepository courseRepository,
            IAdminProfileRepository adminProfileRepository,
            IAuthorizationService authorizationService,
            IUserRepository userRepository,
            IPasswordService passwordService)
        {
            _studentRepository = studentRepository;
            _facultyRepository = facultyRepository;
            _courseRepository = courseRepository;
            _adminProfileRepository = adminProfileRepository;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        public IActionResult Index()
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                try
                {
                    ViewData["TotalStudents"] = _studentRepository.GetCount();
                    ViewData["TotalFaculty"] = _facultyRepository.GetCount();
                    ViewData["TotalCourses"] = _courseRepository.GetCount();
                    return View();
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi khi tải dữ liệu: {ex.Message}";
                    return View();
                }
            });
        }

        public IActionResult Profile()
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                var profile = _adminProfileRepository.Get();
                return View(profile);
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(AdminProfile profile)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                // Validate editable fields: Phone, Address, Gender
                if (profile.Phone != null && profile.Phone.Length > 20)
                {
                    ModelState.AddModelError("Phone", "Phone number cannot exceed 20 characters.");
                }
                if (profile.Address != null && profile.Address.Length > 255)
                {
                    ModelState.AddModelError("Address", "Address cannot exceed 255 characters.");
                }
                if (profile.Gender != null && profile.Gender.Length > 10)
                {
                    ModelState.AddModelError("Gender", "Gender cannot exceed 10 characters.");
                }
                if (profile.Gender != null && !new[] { "Male", "Female", "Other" }.Contains(profile.Gender))
                {
                    ModelState.AddModelError("Gender", "Gender must be Male, Female, or Other.");
                }

                if (!ModelState.IsValid)
                {
                    return View(profile);
                }

                // Get existing profile to preserve read-only fields
                var existingProfile = _adminProfileRepository.Get();
                profile.Username = existingProfile.Username;
                profile.FullName = existingProfile.FullName;
                profile.Email = existingProfile.Email;
                profile.Role = existingProfile.Role;
                // Gender is now editable, so keep the value from the form

                _adminProfileRepository.Save(profile);
                TempData["ProfileUpdated"] = true;
                return RedirectToAction(nameof(Profile));
            });
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                return View(new ChangePasswordViewModel());
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Get current admin user
                var username = HttpContext.Session.GetString("Username");
                if (string.IsNullOrEmpty(username))
                {
                    TempData["ErrorMessage"] = "Session expired. Please login again.";
                    return RedirectToAction("Index", "Login");
                }

                var user = _userRepository.GetByUsername(username);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return View(model);
                }

                // Verify current password
                bool isCurrentPasswordValid = _passwordService.VerifyPassword(model.CurrentPassword, user.Password);
                
                // Backward compatibility: check plain text if hash verification fails
                if (!isCurrentPasswordValid && user.Password == model.CurrentPassword)
                {
                    isCurrentPasswordValid = true;
                }

                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                // Update password with hash
                user.Password = _passwordService.HashPassword(model.NewPassword);
                _userRepository.Update(user);

                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction(nameof(ChangePassword));
            });
        }
    }
}
