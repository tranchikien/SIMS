using SIMS.Models;
using SIMS.Repositories;
using SIMS.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace SIMS.Services
{
    /// <summary>
    /// Implementation of course service (SOLID: Single Responsibility)
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;
        private readonly IGradeRepository _gradeRepository;

        public CourseService(
            ICourseRepository courseRepository,
            IEnrollmentRepository enrollmentRepository,
            IGradeRepository gradeRepository)
        {
            _courseRepository = courseRepository;
            _enrollmentRepository = enrollmentRepository;
            _gradeRepository = gradeRepository;
        }

        public IEnumerable<Course> GetAllCourses(string? searchString = null)
        {
            var courses = _courseRepository.GetAll();

            if (!string.IsNullOrEmpty(searchString))
            {
                courses = courses.Where(c =>
                    c.CourseCode.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    c.CourseName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    (c.Description != null && c.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase)));
            }

            return courses;
        }

        public (bool Success, string? ErrorMessage) CreateCourse(Course course)
        {
            // Validate uniqueness
            if (!IsCourseCodeUnique(course.CourseCode))
            {
                return (false, "Course Code already exists.");
            }

            course.Status = course.Status ?? "Active";
            _courseRepository.Add(course);

            return (true, null);
        }

        public (bool Success, string? ErrorMessage) UpdateCourse(int id, Course course)
        {
            var existingCourse = _courseRepository.GetById(id);
            if (existingCourse == null)
            {
                return (false, "Course not found.");
            }

            // Validate uniqueness (excluding current course)
            if (!IsCourseCodeUnique(course.CourseCode, id))
            {
                return (false, "Course Code already exists.");
            }

            existingCourse.CourseCode = course.CourseCode;
            existingCourse.CourseName = course.CourseName;
            existingCourse.Description = course.Description;
            existingCourse.Credits = course.Credits;
            existingCourse.Status = course.Status;

            _courseRepository.Update(existingCourse);

            return (true, null);
        }

        public (int EnrollmentsCount, int GradesCount) GetCourseDeletionImpact(int courseId)
        {
            var enrollments = _enrollmentRepository.GetByCourseId(courseId).ToList();
            var gradesCount = 0;
            
            foreach (var enrollment in enrollments)
            {
                var grade = _gradeRepository.GetByEnrollmentId(enrollment.Id);
                if (grade != null)
                {
                    gradesCount++;
                }
            }

            return (enrollments.Count, gradesCount);
        }

        public bool DeleteCourse(int id)
        {
            var course = _courseRepository.GetById(id);
            if (course == null)
            {
                return false;
            }

            // Get all enrollments for this course
            var enrollments = _enrollmentRepository.GetByCourseId(id).ToList();
            
            // Delete all grades associated with these enrollments first
            foreach (var enrollment in enrollments)
            {
                var grade = _gradeRepository.GetByEnrollmentId(enrollment.Id);
                if (grade != null)
                {
                    _gradeRepository.Delete(grade.Id);
                }
            }

            // Delete all enrollments for this course
            foreach (var enrollment in enrollments)
            {
                _enrollmentRepository.Delete(enrollment.Id);
            }

            // Now safe to delete the course
            _courseRepository.Delete(id);
            return true;
        }

        public bool IsCourseCodeUnique(string courseCode, int? excludeId = null)
        {
            var existing = _courseRepository.GetAll().FirstOrDefault(c => c.CourseCode == courseCode);
            return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
        }

        public Course? GetCourseById(int id)
        {
            return _courseRepository.GetById(id);
        }
    }
}

