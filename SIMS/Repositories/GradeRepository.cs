using SIMS.Data;
using SIMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Repositories
{
    public class GradeRepository : IGradeRepository
    {
        private readonly SIMSDbContext _context;

        public GradeRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Grade> GetAll()
        {
            return _context.Grades.ToList();
        }

        public Grade? GetById(int id)
        {
            return _context.Grades.FirstOrDefault(g => g.Id == id);
        }

        public Grade? GetByEnrollmentId(int enrollmentId)
        {
            return _context.Grades.FirstOrDefault(g => g.EnrollmentId == enrollmentId);
        }

        public IEnumerable<Grade> GetByStudentId(int studentId)
        {
            return _context.Grades.Where(g => g.StudentId == studentId).ToList();
        }

        public IEnumerable<Grade> GetByCourseId(int courseId)
        {
            return _context.Grades.Where(g => g.CourseId == courseId).ToList();
        }

        public Grade? GetByStudentAndCourse(int studentId, int courseId)
        {
            return _context.Grades.FirstOrDefault(g => g.StudentId == studentId && g.CourseId == courseId);
        }

        public void Add(Grade grade)
        {
            _context.Grades.Add(grade);
            _context.SaveChanges();
        }

        public void Update(Grade grade)
        {
            _context.Grades.Update(grade);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var grade = _context.Grades.FirstOrDefault(g => g.Id == id);
            if (grade != null)
            {
                _context.Grades.Remove(grade);
                _context.SaveChanges();
            }
        }
    }
}

