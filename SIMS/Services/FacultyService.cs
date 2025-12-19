using SIMS.Models;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SIMS.Services
{
    /// <summary>
    /// Implementation of faculty service (SOLID: Single Responsibility)
    /// </summary>
    public class FacultyService : IFacultyService
    {
        private readonly IFacultyRepository _facultyRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IGradeRepository _gradeRepository;
        private readonly IPasswordService _passwordService;

        public FacultyService(
            IFacultyRepository facultyRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository,
            IGradeRepository gradeRepository,
            IPasswordService passwordService)
        {
            _facultyRepository = facultyRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
            _gradeRepository = gradeRepository;
            _passwordService = passwordService;
        }

        public IEnumerable<Faculty> GetAllFaculties(string? searchString = null)
        {
            var faculties = _facultyRepository.GetAll();

            if (!string.IsNullOrEmpty(searchString))
            {
                faculties = faculties.Where(f =>
                    f.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    f.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    f.FacultyId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    f.Department.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            return faculties;
        }

        public (bool Success, string? ErrorMessage) CreateFaculty(FacultyViewModel model)
        {
            // Validate uniqueness
            if (!IsFacultyIdUnique(model.FacultyId))
            {
                return (false, "Faculty ID already exists.");
            }

            if (!IsEmailUnique(model.Email))
            {
                return (false, "Email already exists.");
            }

            // Create faculty
            var faculty = new Faculty
            {
                FullName = model.FullName,
                Email = model.Email,
                FacultyId = model.FacultyId,
                Department = model.Department,
                Status = "Active"
            };

            _facultyRepository.Add(faculty);

            // Create corresponding User account with hashed password
            var user = new User
            {
                Username = model.FacultyId,
                Password = _passwordService.HashPassword(model.Password),
                FullName = model.FullName,
                Email = model.Email,
                Role = "Faculty",
                ReferenceId = faculty.Id,
                Status = "Active",
                Phone = model.Phone,
                Address = model.Address,
                Gender = model.Gender
            };
            _userRepository.Add(user);

            return (true, null);
        }

        public (bool Success, string? ErrorMessage) UpdateFaculty(int id, FacultyEditViewModel model)
        {
            var faculty = _facultyRepository.GetById(id);
            if (faculty == null)
            {
                return (false, "Faculty not found.");
            }

            // Validate uniqueness (excluding current faculty)
            if (!IsFacultyIdUnique(model.FacultyId, id))
            {
                return (false, "Faculty ID already exists.");
            }

            if (!IsEmailUnique(model.Email, id))
            {
                return (false, "Email already exists.");
            }

            // Update faculty
            faculty.FullName = model.FullName;
            faculty.Email = model.Email;
            faculty.FacultyId = model.FacultyId;
            faculty.Department = model.Department;
            faculty.Status = model.Status;

            _facultyRepository.Update(faculty);

            // Update corresponding User account
            var user = _userRepository.GetByReferenceId(faculty.Id, "Faculty");
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Username = model.FacultyId;
                user.Status = model.Status;
                user.Phone = model.Phone;
                user.Address = model.Address;
                user.Gender = model.Gender;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = _passwordService.HashPassword(model.Password);
                }
                _userRepository.Update(user);
            }

            return (true, null);
        }

        public (int EnrollmentsCount, int GradesCount) GetFacultyDeletionImpact(int facultyId)
        {
            var enrollments = _enrollmentRepository.GetByFacultyId(facultyId).ToList();
            var grades = _gradeRepository.GetByFacultyId(facultyId).ToList();

            return (enrollments.Count, grades.Count);
        }

        public bool DeleteFaculty(int id)
        {
            var faculty = _facultyRepository.GetById(id);
            if (faculty == null)
            {
                return false;
            }

            // Set FacultyId to NULL for all grades referencing this faculty
            // This preserves the grades but removes the faculty reference
            var grades = _gradeRepository.GetByFacultyId(id).ToList();
            foreach (var grade in grades)
            {
                grade.FacultyId = null;
                _gradeRepository.Update(grade);
            }

            // Set FacultyId to NULL for all enrollments referencing this faculty
            var enrollments = _enrollmentRepository.GetByFacultyId(id);
            foreach (var enrollment in enrollments)
            {
                enrollment.FacultyId = null;
                _enrollmentRepository.Update(enrollment);
            }

            // Delete corresponding User account
            var user = _userRepository.GetByReferenceId(id, "Faculty");
            if (user != null)
            {
                _userRepository.Delete(user.Id);
            }

            _facultyRepository.Delete(id);
            return true;
        }

        public bool IsFacultyIdUnique(string facultyId, int? excludeId = null)
        {
            var existing = _facultyRepository.GetByFacultyId(facultyId);
            return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
        }

        public bool IsEmailUnique(string email, int? excludeId = null)
        {
            var existing = _facultyRepository.GetByEmail(email);
            return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
        }

        public Faculty? GetFacultyById(int id)
        {
            return _facultyRepository.GetById(id);
        }
    }
}

