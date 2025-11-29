using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Services
{
    /// <summary>
    /// Service for course business logic (SOLID: Single Responsibility)
    /// </summary>
    public interface ICourseService
    {
        /// <summary>
        /// Gets all courses with optional search filter
        /// </summary>
        IEnumerable<Course> GetAllCourses(string? searchString = null);
        
        /// <summary>
        /// Creates a new course
        /// </summary>
        (bool Success, string? ErrorMessage) CreateCourse(Course course);
        
        /// <summary>
        /// Updates an existing course
        /// </summary>
        (bool Success, string? ErrorMessage) UpdateCourse(int id, Course course);
        
        /// <summary>
        /// Deletes a course
        /// </summary>
        bool DeleteCourse(int id);
        
        /// <summary>
        /// Validates if course code is unique
        /// </summary>
        bool IsCourseCodeUnique(string courseCode, int? excludeId = null);
        
        /// <summary>
        /// Gets course by ID
        /// </summary>
        Course? GetCourseById(int id);
    }
}

