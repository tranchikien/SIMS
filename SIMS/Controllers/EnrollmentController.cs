using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Controllers
{
    /// <summary>
    /// Controller for managing enrollments (SOLID: Single Responsibility - Only handles HTTP requests/responses)
    /// </summary>
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IAuthorizationService _authorizationService;

        public EnrollmentController(
            IEnrollmentService enrollmentService,
            IAuthorizationService authorizationService)
        {
            _enrollmentService = enrollmentService;
            _authorizationService = authorizationService;
        }

        // GET: Enrollment
        public IActionResult Index(string searchString)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Manage Enrollments";
                ViewData["breadcrumb"] = "Manage Enrollments";
                ViewData["breadcrumb-item"] = "enrollments";

                var enrollmentList = _enrollmentService.GetEnrollmentsWithDetails(searchString);
                ViewData["EnrollmentList"] = enrollmentList;
                return View();
            });
        }

        // GET: Enrollment/Create
        public IActionResult Create()
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Add Enrollment";
                ViewData["breadcrumb"] = "Add Enrollment";
                ViewData["breadcrumb-item"] = "enrollments";
                ViewData["Students"] = _enrollmentService.GetAllStudents().ToList();
                ViewData["Courses"] = _enrollmentService.GetAllCourses().ToList();
                ViewData["Faculties"] = _enrollmentService.GetAllFaculties().ToList();
                ViewData["Enrollments"] = _enrollmentService.GetEnrollmentsWithDetails();

                return View();
            });
        }

        // POST: Enrollment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EnrollmentViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _enrollmentService.CreateEnrollment(model);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Enrollment added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred while creating the enrollment.");
                }

                ViewData["Title"] = "Add Enrollment";
                ViewData["breadcrumb"] = "Add Enrollment";
                ViewData["breadcrumb-item"] = "enrollments";
                ViewData["Students"] = _enrollmentService.GetAllStudents().ToList();
                ViewData["Courses"] = _enrollmentService.GetAllCourses().ToList();
                ViewData["Faculties"] = _enrollmentService.GetAllFaculties().ToList();
                ViewData["Enrollments"] = _enrollmentService.GetEnrollmentsWithDetails();

                return View(model);
            });
        }

        // POST: Enrollment/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (_enrollmentService.DeleteEnrollment(id))
                {
                    TempData["SuccessMessage"] = "Enrollment deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Enrollment not found!";
                }

                return RedirectToAction(nameof(Index));
            });
        }
    }
}

