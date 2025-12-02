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

        public FacultyService(
            IFacultyRepository facultyRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _facultyRepository = facultyRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
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

            // Create corresponding User account
            var user = new User
            {
                Username = model.FacultyId,
                Password = model.Password,
                FullName = model.FullName,
                Email = model.Email,
                Role = "Faculty",
                ReferenceId = faculty.Id,
                Status = "Active"
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
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }
                _userRepository.Update(user);
            }

            return (true, null);
        }

        public bool DeleteFaculty(int id)
        {
            var faculty = _facultyRepository.GetById(id);
            if (faculty == null)
            {
                return false;
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

