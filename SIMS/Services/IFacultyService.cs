using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Services
{
    /// <summary>
    /// Service for faculty business logic (SOLID: Single Responsibility)
    /// </summary>
    public interface IFacultyService
    {
        /// <summary>
        /// Gets all faculties with optional search filter
        /// </summary>
        IEnumerable<Faculty> GetAllFaculties(string? searchString = null);
        
        /// <summary>
        /// Creates a new faculty and corresponding user account
        /// </summary>
        (bool Success, string? ErrorMessage) CreateFaculty(FacultyViewModel model);
        
        /// <summary>
        /// Updates an existing faculty and corresponding user account
        /// </summary>
        (bool Success, string? ErrorMessage) UpdateFaculty(int id, FacultyEditViewModel model);
        
        /// <summary>
        /// Deletes a faculty and corresponding user account
        /// </summary>
        bool DeleteFaculty(int id);
        
        /// <summary>
        /// Validates if faculty ID is unique
        /// </summary>
        bool IsFacultyIdUnique(string facultyId, int? excludeId = null);
        
        /// <summary>
        /// Validates if email is unique
        /// </summary>
        bool IsEmailUnique(string email, int? excludeId = null);
        
        /// <summary>
        /// Gets faculty by ID
        /// </summary>
        Faculty? GetFacultyById(int id);
    }
}

