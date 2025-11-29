using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Services;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace SIMS.Controllers
{
    public class StudentDashboardController : Controller
    {
        private readonly IStudentDashboardService _studentDashboardService;
        private readonly IStudentRepository _studentRepository;

        public StudentDashboardController(
            IStudentDashboardService studentDashboardService,
            IStudentRepository studentRepository)
        {
            _studentDashboardService = studentDashboardService;
            _studentRepository = studentRepository;
        }

        private Student? GetCurrentStudent()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int studentId))
            {
                return null;
            }

            return _studentRepository.GetById(studentId);
        }


        public IActionResult Index()
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var dashboardInfo = _studentDashboardService.GetDashboardInfo(student.Id);

            ViewData["Title"] = "Student Dashboard";
            ViewData["breadcrumb"] = "Student Dashboard";
            ViewData["breadcrumb-item"] = "student-dashboard";
            ViewData["Student"] = dashboardInfo.Student;
            ViewData["EnrolledCourses"] = dashboardInfo.EnrolledCourses;
            ViewData["TotalCredits"] = dashboardInfo.TotalCredits;
            ViewData["GPA"] = dashboardInfo.GPA;

            return View();
        }

        public IActionResult Profile()
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewData["Title"] = "My Profile";
            ViewData["breadcrumb"] = "My Profile";
            ViewData["breadcrumb-item"] = "my-profile";
            return View(student);
        }

        public IActionResult MyCourses()
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var enrolledCourses = _studentDashboardService.GetEnrolledCourses(student.Id);

            ViewData["Title"] = "My Courses";
            ViewData["breadcrumb"] = "My Courses";
            ViewData["breadcrumb-item"] = "my-courses";
            ViewData["EnrolledCourses"] = enrolledCourses;

            return View();
        }

        public IActionResult MyGrades()
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var gradeDetails = _studentDashboardService.GetGrades(student.Id);

            ViewData["Title"] = "My Grades";
            ViewData["breadcrumb"] = "My Grades";
            ViewData["breadcrumb-item"] = "my-grades";
            return View(gradeDetails);
        }
    }
}

