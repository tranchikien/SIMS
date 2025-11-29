using SIMS.Data;
using SIMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Repositories
{
    public class EnrollmentRepository : IEnrollmentRepository
    {
        private readonly SIMSDbContext _context;

        public EnrollmentRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Enrollment> GetAll()
        {
            return _context.Enrollments.ToList();
        }

        public Enrollment? GetById(int id)
        {
            return _context.Enrollments.FirstOrDefault(e => e.Id == id);
        }

        public IEnumerable<Enrollment> GetByStudentId(int studentId)
        {
            return _context.Enrollments.Where(e => e.StudentId == studentId).ToList();
        }

        public IEnumerable<Enrollment> GetByCourseId(int courseId)
        {
            return _context.Enrollments.Where(e => e.CourseId == courseId).ToList();
        }

        public IEnumerable<Enrollment> GetByFacultyId(int facultyId)
        {
            return _context.Enrollments.Where(e => e.FacultyId == facultyId).ToList();
        }

        public void Add(Enrollment enrollment)
        {
            _context.Enrollments.Add(enrollment);
            _context.SaveChanges();
        }

        public void Update(Enrollment enrollment)
        {
            _context.Enrollments.Update(enrollment);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var enrollment = _context.Enrollments.FirstOrDefault(e => e.Id == id);
            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                _context.SaveChanges();
            }
        }
    }
}

