using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Controllers
{
    /// <summary>
    /// Controller for managing students (SOLID: Single Responsibility - Only handles HTTP requests/responses)
    /// </summary>
    public class StudentController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserRepository _userRepository;

        public StudentController(
            IStudentRepository studentRepository, 
            IStudentService studentService,
            IAuthorizationService authorizationService,
            IUserRepository userRepository)
        {
            _studentRepository = studentRepository;
            _studentService = studentService;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
        }

        // Public property to get count for Dashboard
        public int GetCount() => _studentRepository.GetCount();
        
        // Public method to get all students for login
        public List<Student> GetAllStudents() => _studentRepository.GetAll().ToList();

        // GET: Student
        public IActionResult Index(string searchString)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Manage Students";
                ViewData["breadcrumb"] = "Manage Students";
                ViewData["breadcrumb-item"] = "students";

                var students = _studentService.GetAllStudents(searchString);
                return View(students.ToList());
            });
        }

        // GET: Student/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Student";
            ViewData["breadcrumb"] = "Add Student";
            ViewData["breadcrumb-item"] = "students";
            return View();
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _studentService.CreateStudent(model);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Student added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred while creating the student.");
                }

                ViewData["Title"] = "Add Student";
                ViewData["breadcrumb"] = "Add Student";
                ViewData["breadcrumb-item"] = "students";
                return View(model);
            });
        }

        // GET: Student/Edit/5
        public IActionResult Edit(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                var student = _studentService.GetStudentById(id);
                if (student == null)
                {
                    return NotFound();
                }

                // Get User information for Phone, Address, Gender
                var user = _userRepository.GetByReferenceId(student.Id, "Student");
                var model = new StudentEditViewModel
                {
                    Id = student.Id,
                    FullName = student.FullName,
                    Email = student.Email,
                    StudentId = student.StudentId,
                    Program = student.Program,
                    Password = null, // Don't pre-fill password for security
                    ConfirmPassword = null,
                    Role = "Student", // Role is always Student for students
                    Status = student.Status,
                    Phone = user?.Phone,
                    Address = user?.Address,
                    Gender = user?.Gender
                };

                ViewData["Title"] = "Edit Student";
                ViewData["breadcrumb"] = "Edit Student";
                ViewData["breadcrumb-item"] = "students";
                return View(model);
            });
        }

        // POST: Student/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, StudentEditViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (id != model.Id)
                {
                    return NotFound();
                }

                // Only validate password if it's being changed
                if (string.IsNullOrWhiteSpace(model.Password))
                {
                    ModelState.Remove("Password");
                    ModelState.Remove("ConfirmPassword");
                    model.Password = null;
                    model.ConfirmPassword = null;
                }
                else
                {
                    if (model.Password.Length < 6)
                    {
                        ModelState.AddModelError("Password", "Password must be at least 6 characters.");
                    }
                    if (string.IsNullOrWhiteSpace(model.ConfirmPassword) || model.Password != model.ConfirmPassword)
                    {
                        ModelState.AddModelError("ConfirmPassword", "Password and confirmation password do not match.");
                    }
                }

                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _studentService.UpdateStudent(id, model);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Student updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred while updating the student.");
                }

                ViewData["Title"] = "Edit Student";
                ViewData["breadcrumb"] = "Edit Student";
                ViewData["breadcrumb-item"] = "students";
                return View(model);
            });
        }

        // POST: Student/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (_studentService.DeleteStudent(id))
                {
                    TempData["SuccessMessage"] = "Student deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Student not found!";
                }

                return RedirectToAction(nameof(Index));
            });
        }
    }
}

