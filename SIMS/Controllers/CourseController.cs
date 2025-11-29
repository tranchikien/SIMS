using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Controllers
{
    /// <summary>
    /// Controller for managing courses (SOLID: Single Responsibility - Only handles HTTP requests/responses)
    /// </summary>
    public class CourseController : Controller
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseService _courseService;
        private readonly IAuthorizationService _authorizationService;

        public CourseController(
            ICourseRepository courseRepository,
            ICourseService courseService,
            IAuthorizationService authorizationService)
        {
            _courseRepository = courseRepository;
            _courseService = courseService;
            _authorizationService = authorizationService;
        }

        // Public property to get count for Dashboard
        public int GetCount() => _courseRepository.GetCount();

        // GET: Course
        public IActionResult Index(string searchString)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Manage Courses";
                ViewData["breadcrumb"] = "Manage Courses";
                ViewData["breadcrumb-item"] = "courses";

                var courses = _courseService.GetAllCourses(searchString);
                return View(courses.ToList());
            });
        }

        // GET: Course/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Course";
            ViewData["breadcrumb"] = "Add Course";
            ViewData["breadcrumb-item"] = "courses";
            return View();
        }

        // POST: Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Course course)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _courseService.CreateCourse(course);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Course added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError("CourseCode", errorMessage ?? "An error occurred while creating the course.");
                }

                ViewData["Title"] = "Add Course";
                ViewData["breadcrumb"] = "Add Course";
                ViewData["breadcrumb-item"] = "courses";
                return View(course);
            });
        }

        // GET: Course/Edit/5
        public IActionResult Edit(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                var course = _courseService.GetCourseById(id);
                if (course == null)
                {
                    return NotFound();
                }

                ViewData["Title"] = "Edit Course";
                ViewData["breadcrumb"] = "Edit Course";
                ViewData["breadcrumb-item"] = "courses";
                return View(course);
            });
        }

        // POST: Course/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Course course)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (id != course.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _courseService.UpdateCourse(id, course);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Course updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError("CourseCode", errorMessage ?? "An error occurred while updating the course.");
                }

                ViewData["Title"] = "Edit Course";
                ViewData["breadcrumb"] = "Edit Course";
                ViewData["breadcrumb-item"] = "courses";
                return View(course);
            });
        }

        // POST: Course/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (_courseService.DeleteCourse(id))
                {
                    TempData["SuccessMessage"] = "Course deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Course not found!";
                }

                return RedirectToAction(nameof(Index));
            });
        }
    }
}

