using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Controllers
{
    /// <summary>
    /// Controller for managing faculties (SOLID: Single Responsibility - Only handles HTTP requests/responses)
    /// </summary>
    public class FacultyController : Controller
    {
        private readonly IFacultyRepository _facultyRepository;
        private readonly IFacultyService _facultyService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserRepository _userRepository;

        public FacultyController(
            IFacultyRepository facultyRepository, 
            IFacultyService facultyService,
            IAuthorizationService authorizationService,
            IUserRepository userRepository)
        {
            _facultyRepository = facultyRepository;
            _facultyService = facultyService;
            _authorizationService = authorizationService;
            _userRepository = userRepository;
        }

        // Public property to get count for Dashboard
        public int GetCount() => _facultyRepository.GetCount();
        
        // Public method to get all faculties for login
        public List<Faculty> GetAllFaculties() => _facultyRepository.GetAll().ToList();

        // GET: Faculty
        public IActionResult Index(string searchString)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Manage Faculty";
                ViewData["breadcrumb"] = "Manage Faculty";
                ViewData["breadcrumb-item"] = "faculty";

                var faculties = _facultyService.GetAllFaculties(searchString);
                return View(faculties.ToList());
            });
        }

        // GET: Faculty/Create
        public IActionResult Create()
        {
            ViewData["Title"] = "Add Faculty";
            ViewData["breadcrumb"] = "Add Faculty";
            ViewData["breadcrumb-item"] = "faculty";
            return View();
        }

        // POST: Faculty/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(FacultyViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _facultyService.CreateFaculty(model);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Faculty added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred while creating the faculty.");
                }

                ViewData["Title"] = "Add Faculty";
                ViewData["breadcrumb"] = "Add Faculty";
                ViewData["breadcrumb-item"] = "faculty";
                return View(model);
            });
        }

        // GET: Faculty/Edit/5
        public IActionResult Edit(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                var faculty = _facultyService.GetFacultyById(id);
                if (faculty == null)
                {
                    return NotFound();
                }

                // Get User information for Phone, Address, Gender
                var user = _userRepository.GetByReferenceId(faculty.Id, "Faculty");
                var model = new FacultyEditViewModel
                {
                    Id = faculty.Id,
                    FullName = faculty.FullName,
                    Email = faculty.Email,
                    FacultyId = faculty.FacultyId,
                    Department = faculty.Department,
                    Password = null, // Don't pre-fill password for security
                    ConfirmPassword = null,
                    Role = "Faculty", // Role is always Faculty for faculties
                    Status = faculty.Status,
                    Phone = user?.Phone,
                    Address = user?.Address,
                    Gender = user?.Gender
                };

                ViewData["Title"] = "Edit Faculty";
                ViewData["breadcrumb"] = "Edit Faculty";
                ViewData["breadcrumb-item"] = "faculty";
                return View(model);
            });
        }

        // POST: Faculty/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, FacultyEditViewModel model)
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
                    var (success, errorMessage) = _facultyService.UpdateFaculty(id, model);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Faculty updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred while updating the faculty.");
                }

                ViewData["Title"] = "Edit Faculty";
                ViewData["breadcrumb"] = "Edit Faculty";
                ViewData["breadcrumb-item"] = "faculty";
                return View(model);
            });
        }

        // GET: Faculty/DeleteImpact/5
        [HttpGet]
        public IActionResult GetDeletionImpact(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                var impact = _facultyService.GetFacultyDeletionImpact(id);
                var faculty = _facultyService.GetFacultyById(id);
                
                return Json(new
                {
                    enrollmentsCount = impact.EnrollmentsCount,
                    gradesCount = impact.GradesCount,
                    facultyName = faculty?.FullName ?? "Unknown",
                    note = "Faculty references in enrollments and grades will be set to NULL, but records will be preserved."
                });
            });
        }

        // POST: Faculty/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (_facultyService.DeleteFaculty(id))
                {
                    TempData["SuccessMessage"] = "Faculty deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Faculty not found!";
                }

                return RedirectToAction(nameof(Index));
            });
        }
    }
}

