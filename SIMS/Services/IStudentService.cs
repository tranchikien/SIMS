using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Services
{
    /// <summary>
    /// Service for student business logic (SOLID: Single Responsibility)
    /// </summary>
    public interface IStudentService
    {
        /// <summary>
        /// Gets all students with optional search filter
        /// </summary>
        IEnumerable<Student> GetAllStudents(string? searchString = null);
        
        /// <summary>
        /// Creates a new student and corresponding user account
        /// </summary>
        (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model);
        
        /// <summary>
        /// Updates an existing student and corresponding user account
        /// </summary>
        (bool Success, string? ErrorMessage) UpdateStudent(int id, StudentEditViewModel model);
        
        /// <summary>
        /// Deletes a student and corresponding user account
        /// </summary>
        bool DeleteStudent(int id);
        
        /// <summary>
        /// Validates if student ID is unique
        /// </summary>
        bool IsStudentIdUnique(string studentId, int? excludeId = null);
        
        /// <summary>
        /// Validates if email is unique
        /// </summary>
        bool IsEmailUnique(string email, int? excludeId = null);
        
        /// <summary>
        /// Gets student by ID
        /// </summary>
        Student? GetStudentById(int id);
    }
}

