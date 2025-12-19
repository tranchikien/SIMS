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
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public StudentDashboardController(
            IStudentDashboardService studentDashboardService,
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            IPasswordService passwordService)
        {
            _studentDashboardService = studentDashboardService;
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _passwordService = passwordService;
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

            // Get User information for Phone, Address, Gender
            var user = _userRepository.GetByReferenceId(student.Id, "Student");
            var viewModel = new StudentProfileViewModel
            {
                Id = student.Id,
                StudentId = student.StudentId,
                FullName = student.FullName,
                Email = student.Email,
                Program = student.Program,
                Status = student.Status,
                Phone = user?.Phone,
                Address = user?.Address,
                Gender = user?.Gender
            };

            ViewData["Title"] = "My Profile";
            ViewData["breadcrumb"] = "My Profile";
            ViewData["breadcrumb-item"] = "my-profile";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(StudentProfileViewModel model)
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Validate editable fields: Phone, Address, Gender
            if (model.Phone != null && model.Phone.Length > 20)
            {
                ModelState.AddModelError("Phone", "Phone number cannot exceed 20 characters.");
            }
            if (model.Address != null && model.Address.Length > 255)
            {
                ModelState.AddModelError("Address", "Address cannot exceed 255 characters.");
            }
            if (model.Gender != null && model.Gender.Length > 10)
            {
                ModelState.AddModelError("Gender", "Gender cannot exceed 10 characters.");
            }
            if (model.Gender != null && !string.IsNullOrEmpty(model.Gender) && !new[] { "Male", "Female", "Other" }.Contains(model.Gender))
            {
                ModelState.AddModelError("Gender", "Gender must be Male, Female, or Other.");
            }

            if (!ModelState.IsValid)
            {
                // Reload read-only fields
                model.StudentId = student.StudentId;
                model.FullName = student.FullName;
                model.Email = student.Email;
                model.Program = student.Program;
                model.Status = student.Status;
                return View(model);
            }

            // Update User Phone, Address, and Gender
            var userToUpdate = _userRepository.GetByReferenceId(student.Id, "Student");
            if (userToUpdate != null)
            {
                userToUpdate.Phone = model.Phone;
                userToUpdate.Address = model.Address;
                userToUpdate.Gender = model.Gender; // Now editable
                _userRepository.Update(userToUpdate);
            }

            TempData["ProfileUpdated"] = true;
            return RedirectToAction(nameof(Profile));
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            ViewData["Title"] = "Change Password";
            ViewData["breadcrumb"] = "Change Password";
            ViewData["breadcrumb-item"] = "change-password";
            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var student = GetCurrentStudent();
            if (student == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Change Password";
                ViewData["breadcrumb"] = "Change Password";
                ViewData["breadcrumb-item"] = "change-password";
                return View(model);
            }

            // Get current user
            var user = _userRepository.GetByReferenceId(student.Id, "Student");
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
                ViewData["Title"] = "Change Password";
                ViewData["breadcrumb"] = "Change Password";
                ViewData["breadcrumb-item"] = "change-password";
                return View(model);
            }

            // Update password with hash
            user.Password = _passwordService.HashPassword(model.NewPassword);
            _userRepository.Update(user);

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(ChangePassword));
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

