using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using System;

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

        public DashboardController(
            IStudentRepository studentRepository,
            IFacultyRepository facultyRepository,
            ICourseRepository courseRepository,
            IAdminProfileRepository adminProfileRepository,
            IAuthorizationService authorizationService)
        {
            _studentRepository = studentRepository;
            _facultyRepository = facultyRepository;
            _courseRepository = courseRepository;
            _adminProfileRepository = adminProfileRepository;
            _authorizationService = authorizationService;
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
                if (!ModelState.IsValid)
                {
                    return View(profile);
                }

                _adminProfileRepository.Save(profile);
                TempData["ProfileUpdated"] = true;
                return RedirectToAction(nameof(Profile));
            });
        }
    }
}
