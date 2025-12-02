using SIMS.Models;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SIMS.Services
{
    /// <summary>
    /// Implementation of student service (SOLID: Single Responsibility)
    /// </summary>
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public StudentService(
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public IEnumerable<Student> GetAllStudents(string? searchString = null)
        {
            var students = _studentRepository.GetAll();

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s =>
                    s.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentId.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.Program.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            return students;
        }

        public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
        {
            // Validate uniqueness
            if (!IsStudentIdUnique(model.StudentId))
            {
                return (false, "Student ID already exists.");
            }

            if (!IsEmailUnique(model.Email))
            {
                return (false, "Email already exists.");
            }

            // Create student
            var student = new Student
            {
                FullName = model.FullName,
                Email = model.Email,
                StudentId = model.StudentId,
                Program = model.Program,
                Status = "Active"
            };

            _studentRepository.Add(student);

            // Create corresponding User account
            var user = new User
            {
                Username = model.StudentId,
                Password = model.Password,
                FullName = model.FullName,
                Email = model.Email,
                Role = "Student",
                ReferenceId = student.Id,
                Status = "Active"
            };
            _userRepository.Add(user);

            return (true, null);
        }

        public (bool Success, string? ErrorMessage) UpdateStudent(int id, StudentEditViewModel model)
        {
            var student = _studentRepository.GetById(id);
            if (student == null)
            {
                return (false, "Student not found.");
            }

            // Validate uniqueness (excluding current student)
            if (!IsStudentIdUnique(model.StudentId, id))
            {
                return (false, "Student ID already exists.");
            }

            if (!IsEmailUnique(model.Email, id))
            {
                return (false, "Email already exists.");
            }

            // Update student
            student.FullName = model.FullName;
            student.Email = model.Email;
            student.StudentId = model.StudentId;
            student.Program = model.Program;
            student.Status = model.Status;

            _studentRepository.Update(student);

            // Update corresponding User account
            var user = _userRepository.GetByReferenceId(student.Id, "Student");
            if (user != null)
            {
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.Username = model.StudentId;
                user.Status = model.Status;
                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.Password = model.Password;
                }
                _userRepository.Update(user);
            }

            return (true, null);
        }

        public bool DeleteStudent(int id)
        {
            var student = _studentRepository.GetById(id);
            if (student == null)
            {
                return false;
            }

            // Delete all enrollments for this student first (due to foreign key constraint)
            var enrollments = _enrollmentRepository.GetByStudentId(id).ToList();
            foreach (var enrollment in enrollments)
            {
                _enrollmentRepository.Delete(enrollment.Id);
            }

            // Delete corresponding User account
            var user = _userRepository.GetByReferenceId(id, "Student");
            if (user != null)
            {
                _userRepository.Delete(user.Id);
            }

            _studentRepository.Delete(id);
            return true;
        }

        public bool IsStudentIdUnique(string studentId, int? excludeId = null)
        {
            var existing = _studentRepository.GetByStudentId(studentId);
            return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
        }

        public bool IsEmailUnique(string email, int? excludeId = null)
        {
            var existing = _studentRepository.GetByEmail(email);
            return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
        }

        public Student? GetStudentById(int id)
        {
            return _studentRepository.GetById(id);
        }
    }
}

