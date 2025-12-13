using SIMS.Models;
using SIMS.Repositories;
using System.Collections.Generic;
using System.Linq;
using System;

namespace SIMS.Services
{
    /// <summary>
    /// Implementation of enrollment service (SOLID: Single Responsibility)
    /// </summary>
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IFacultyRepository _facultyRepository;

        public EnrollmentService(
            IEnrollmentRepository enrollmentRepository,
            IStudentRepository studentRepository,
            ICourseRepository courseRepository,
            IFacultyRepository facultyRepository)
        {
            _enrollmentRepository = enrollmentRepository;
            _studentRepository = studentRepository;
            _courseRepository = courseRepository;
            _facultyRepository = facultyRepository;
        }

        public IEnumerable<EnrollmentDisplayViewModel> GetEnrollmentsWithDetails(string? searchString = null)
        {
            var enrollments = _enrollmentRepository.GetAll();
            var students = _studentRepository.GetAll();
            var courses = _courseRepository.GetAll();
            var faculties = _facultyRepository.GetAll();

            var enrollmentList = enrollments.Select(e => new EnrollmentDisplayViewModel
            {
                Id = e.Id,
                StudentId = e.StudentId,
                CourseId = e.CourseId,
                FacultyId = e.FacultyId,
                Status = e.Status,
                StudentName = students.FirstOrDefault(s => s.Id == e.StudentId)?.FullName ?? "",
                StudentCode = students.FirstOrDefault(s => s.Id == e.StudentId)?.StudentId ?? "",
                CourseCode = courses.FirstOrDefault(c => c.Id == e.CourseId)?.CourseCode ?? "",
                CourseName = courses.FirstOrDefault(c => c.Id == e.CourseId)?.CourseName ?? "",
                FacultyName = faculties.FirstOrDefault(f => f.Id == e.FacultyId)?.FullName ?? ""
            }).Where(x => !string.IsNullOrEmpty(x.StudentName) && !string.IsNullOrEmpty(x.CourseName));

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                enrollmentList = enrollmentList.Where(e =>
                    e.StudentName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    e.StudentCode.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    e.CourseCode.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    e.CourseName.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            return enrollmentList;
        }

        public (bool Success, string? ErrorMessage) CreateEnrollment(EnrollmentViewModel model)
        {
            // Check if student is already enrolled in this course
            var existingEnrollment = _enrollmentRepository.GetAll()
                .FirstOrDefault(e => e.StudentId == model.StudentId && 
                                     e.CourseId == model.CourseId && 
                                     e.Status == "Enrolled");

            if (existingEnrollment != null)
            {
                // If enrollment exists but FacultyId is NULL, allow updating the FacultyId
                if (existingEnrollment.FacultyId == null && model.FacultyId.HasValue)
                {
                    existingEnrollment.FacultyId = model.FacultyId;
                    _enrollmentRepository.Update(existingEnrollment);
                    return (true, null);
                }
                // If enrollment exists with a FacultyId, return error
                return (false, "This student is already enrolled in this course.");
            }

            var enrollment = new Enrollment
            {
                StudentId = model.StudentId,
                CourseId = model.CourseId,
                FacultyId = model.FacultyId,
                Status = model.Status ?? "Enrolled"
            };

            _enrollmentRepository.Add(enrollment);

            return (true, null);
        }

        public bool DeleteEnrollment(int id)
        {
            var enrollment = _enrollmentRepository.GetById(id);
            if (enrollment == null)
            {
                return false;
            }

            _enrollmentRepository.Delete(id);
            return true;
        }

        public bool IsStudentEnrolledInCourse(int studentId, int courseId)
        {
            var enrollments = _enrollmentRepository.GetAll();
            return enrollments.Any(e => e.StudentId == studentId && e.CourseId == courseId && e.Status == "Enrolled");
        }

        public Enrollment? GetEnrollmentById(int id)
        {
            return _enrollmentRepository.GetById(id);
        }

        public IEnumerable<Student> GetAllStudents()
        {
            return _studentRepository.GetAll();
        }

        public IEnumerable<Course> GetAllCourses()
        {
            return _courseRepository.GetAll();
        }

        public IEnumerable<Faculty> GetAllFaculties()
        {
            return _facultyRepository.GetAll();
        }

        public (bool Success, string? ErrorMessage) UpdateEnrollmentFaculty(int enrollmentId, int? facultyId)
        {
            var enrollment = _enrollmentRepository.GetById(enrollmentId);
            if (enrollment == null)
            {
                return (false, "Enrollment not found.");
            }

            enrollment.FacultyId = facultyId;
            _enrollmentRepository.Update(enrollment);

            return (true, null);
        }

        public (bool Success, string? ErrorMessage) UpdateEnrollment(int enrollmentId, int? facultyId, string status)
        {
            var enrollment = _enrollmentRepository.GetById(enrollmentId);
            if (enrollment == null)
            {
                return (false, "Enrollment not found.");
            }

            // Validate status
            if (!new[] { "Enrolled", "Completed", "Dropped" }.Contains(status))
            {
                return (false, "Invalid status. Status must be Enrolled, Completed, or Dropped.");
            }

            enrollment.FacultyId = facultyId;
            enrollment.Status = status;
            _enrollmentRepository.Update(enrollment);

            return (true, null);
        }
    }
}

