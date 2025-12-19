using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Services;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace SIMS.Controllers
{
    public class FacultyDashboardController : Controller
    {
        private readonly IFacultyRepository _facultyRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IGradeService _gradeService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;

        public FacultyDashboardController(
            IFacultyRepository facultyRepository,
            IEnrollmentRepository enrollmentRepository,
            ICourseRepository courseRepository,
            IStudentRepository studentRepository,
            IGradeRepository gradeRepository,
            IGradeService gradeService,
            IUserRepository userRepository,
            IPasswordService passwordService)
        {
            _facultyRepository = facultyRepository;
            _enrollmentRepository = enrollmentRepository;
            _courseRepository = courseRepository;
            _studentRepository = studentRepository;
            _gradeRepository = gradeRepository;
            _gradeService = gradeService;
            _userRepository = userRepository;
            _passwordService = passwordService;
        }

        private Faculty? GetCurrentFaculty()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int facultyId))
            {
                return null;
            }

            return _facultyRepository.GetById(facultyId);
        }

        public IActionResult Index()
        {
            var faculty = GetCurrentFaculty();
            if (faculty == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Get enrollments where this faculty is assigned
            var enrollments = _enrollmentRepository.GetByFacultyId(faculty.Id)
                .Where(e => e.Status == "Enrolled")
                .ToList();

            var courses = _courseRepository.GetAll().ToList();
            var students = _studentRepository.GetAll().ToList();

            // Get unique courses taught by this faculty
            var coursesTaught = enrollments
                .Select(e => courses.FirstOrDefault(c => c.Id == e.CourseId))
                .Where(c => c != null)
                .Distinct()
                .ToList();

            // Count unique students
            var uniqueStudents = enrollments
                .Select(e => students.FirstOrDefault(s => s.Id == e.StudentId))
                .Where(s => s != null)
                .Distinct()
                .Count();

            ViewData["Title"] = "Faculty Dashboard";
            ViewData["breadcrumb"] = "Faculty Dashboard";
            ViewData["breadcrumb-item"] = "faculty-dashboard";
            ViewData["Faculty"] = faculty;
            ViewData["CoursesCount"] = coursesTaught.Count;
            ViewData["StudentsCount"] = uniqueStudents;

            return View();
        }

        public IActionResult Profile()
        {
            var faculty = GetCurrentFaculty();
            if (faculty == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Get User information for Phone, Address, Gender
            var user = _userRepository.GetByReferenceId(faculty.Id, "Faculty");
            var viewModel = new FacultyProfileViewModel
            {
                Id = faculty.Id,
                FacultyId = faculty.FacultyId,
                FullName = faculty.FullName,
                Email = faculty.Email,
                Department = faculty.Department,
                Status = faculty.Status,
                Phone = user?.Phone,
                Address = user?.Address,
                Gender = user?.Gender
            };

            ViewData["Title"] = "My Profile";
            ViewData["breadcrumb"] = "My Profile";
            ViewData["breadcrumb-item"] = "";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(FacultyProfileViewModel model)
        {
            var faculty = GetCurrentFaculty();
            if (faculty == null)
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
                model.FacultyId = faculty.FacultyId;
                model.FullName = faculty.FullName;
                model.Email = faculty.Email;
                model.Department = faculty.Department;
                model.Status = faculty.Status;
                return View(model);
            }

            // Update User Phone, Address, and Gender
            var userToUpdate = _userRepository.GetByReferenceId(faculty.Id, "Faculty");
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
            var faculty = GetCurrentFaculty();
            if (faculty == null)
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
            var faculty = GetCurrentFaculty();
            if (faculty == null)
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
            var user = _userRepository.GetByReferenceId(faculty.Id, "Faculty");
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
            var faculty = GetCurrentFaculty();
            if (faculty == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Get enrollments where this faculty is assigned
            var enrollments = _enrollmentRepository.GetByFacultyId(faculty.Id)
                .Where(e => e.Status == "Enrolled")
                .ToList();

            var courses = _courseRepository.GetAll().ToList();
            var students = _studentRepository.GetAll().ToList();

            // Get courses with student count
            var coursesList = enrollments
                .GroupBy(e => e.CourseId)
                .Select(g => new
                {
                    Course = courses.FirstOrDefault(c => c.Id == g.Key),
                    StudentCount = g.Count()
                })
                .Where(x => x.Course != null)
                .Select(x => new
                {
                    CourseId = x.Course.Id,
                    CourseCode = x.Course.CourseCode,
                    CourseName = x.Course.CourseName,
                    Credits = x.Course.Credits,
                    StudentCount = x.StudentCount
                })
                .ToList();

            ViewData["Title"] = "My Courses";
            ViewData["breadcrumb"] = "My Courses";
            ViewData["breadcrumb-item"] = "my-courses";
            ViewData["CoursesList"] = coursesList;

            return View();
        }

        public IActionResult ViewStudents(int courseId)
        {
            var faculty = GetCurrentFaculty();
            if (faculty == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var course = _courseRepository.GetById(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found!";
                return RedirectToAction("MyCourses");
            }

            // Get enrollments for this course and faculty
            var enrollments = _enrollmentRepository.GetByCourseId(courseId)
                .Where(e => e.FacultyId == faculty.Id && e.Status == "Enrolled")
                .ToList();

            var students = _studentRepository.GetAll().ToList();
            var grades = _gradeRepository.GetAll().ToList();

            // Get student list with current scores
            var studentList = enrollments
                .Select(e => new
                {
                    Enrollment = e,
                    Student = students.FirstOrDefault(s => s.Id == e.StudentId),
                    Grade = grades.FirstOrDefault(g => g.EnrollmentId == e.Id)
                })
                .Where(x => x.Student != null)
                .Select(x => new
                {
                    EnrollmentId = x.Enrollment.Id,
                    StudentId = x.Student.StudentId,
                    StudentName = x.Student.FullName,
                    Email = x.Student.Email,
                    CurrentScore = x.Grade?.TotalScore
                })
                .ToList();

            ViewData["Title"] = "View Students";
            ViewData["breadcrumb"] = $"View Students - {course.CourseName}";
            ViewData["breadcrumb-item"] = "view-students";
            ViewData["Course"] = course;
            ViewData["StudentList"] = studentList;
            ViewData["Faculty"] = faculty; // Thêm thông tin giảng viên vào ViewData

            return View();
        }

        [HttpGet]
        public IActionResult GradeStudents(int courseId)
        {
            var faculty = GetCurrentFaculty();
            if (faculty == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var course = _courseRepository.GetById(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found!";
                return RedirectToAction("MyCourses");
            }

            // Get enrollments for this course and faculty
            var enrollments = _enrollmentRepository.GetByCourseId(courseId)
                .Where(e => e.FacultyId == faculty.Id && e.Status == "Enrolled")
                .ToList();

            var students = _studentRepository.GetAll().ToList();
            var grades = _gradeRepository.GetAll().ToList();

            // Create grade view model
            var gradeViewModel = new GradeViewModel
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                Students = enrollments
                    .Select(e => new StudentGradeItem
                    {
                        EnrollmentId = e.Id,
                        StudentId = e.StudentId,
                        StudentIdCode = students.FirstOrDefault(s => s.Id == e.StudentId)?.StudentId ?? "",
                        StudentName = students.FirstOrDefault(s => s.Id == e.StudentId)?.FullName ?? "",
                        FinalScore = grades.FirstOrDefault(g => g.EnrollmentId == e.Id)?.FinalScore,
                        Comment = grades.FirstOrDefault(g => g.EnrollmentId == e.Id)?.Comment
                    })
                    .ToList()
            };

            ViewData["Title"] = "Grade Students";
            ViewData["breadcrumb"] = $"Grade Students - {course.CourseName}";
            ViewData["breadcrumb-item"] = "grade-students";
            ViewData["Faculty"] = faculty; // Thêm thông tin giảng viên vào ViewData

            return View(gradeViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GradeStudents(GradeViewModel model)
        {
            var faculty = GetCurrentFaculty();
            if (faculty == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                _gradeService.SaveGrades(model, faculty.Id);
                
                TempData["SuccessMessage"] = "Grades saved successfully! Student GPA will be updated when they view their dashboard.";
                return RedirectToAction("ViewStudents", new { courseId = model.CourseId });
            }

            // Reload course info if validation fails
            var course = _courseRepository.GetById(model.CourseId);
            if (course != null)
            {
                ViewData["Title"] = "Grade Students";
                ViewData["breadcrumb"] = $"Grade Students - {course.CourseName}";
                ViewData["breadcrumb-item"] = "grade-students";
                ViewData["Faculty"] = faculty; // Thêm thông tin giảng viên vào ViewData
            }

            return View(model);
        }
    }
}

