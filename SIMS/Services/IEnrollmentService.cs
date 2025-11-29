using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Services
{
    /// <summary>
    /// Service for enrollment business logic (SOLID: Single Responsibility)
    /// </summary>
    public interface IEnrollmentService
    {
        /// <summary>
        /// Gets all enrollments with related data and optional search filter
        /// </summary>
        IEnumerable<EnrollmentDisplayViewModel> GetEnrollmentsWithDetails(string? searchString = null);
        
        /// <summary>
        /// Creates a new enrollment
        /// </summary>
        (bool Success, string? ErrorMessage) CreateEnrollment(EnrollmentViewModel model);
        
        /// <summary>
        /// Deletes an enrollment
        /// </summary>
        bool DeleteEnrollment(int id);
        
        /// <summary>
        /// Validates if student is already enrolled in course
        /// </summary>
        bool IsStudentEnrolledInCourse(int studentId, int courseId);
        
        /// <summary>
        /// Gets enrollment by ID
        /// </summary>
        Enrollment? GetEnrollmentById(int id);
        
        /// <summary>
        /// Gets all students for dropdown
        /// </summary>
        IEnumerable<Student> GetAllStudents();
        
        /// <summary>
        /// Gets all courses for dropdown
        /// </summary>
        IEnumerable<Course> GetAllCourses();
        
        /// <summary>
        /// Gets all faculties for dropdown
        /// </summary>
        IEnumerable<Faculty> GetAllFaculties();
    }
}

