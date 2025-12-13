using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface IGradeRepository
    {
        IEnumerable<Grade> GetAll();
        Grade? GetById(int id);
        Grade? GetByEnrollmentId(int enrollmentId);
        IEnumerable<Grade> GetByStudentId(int studentId);
        IEnumerable<Grade> GetByCourseId(int courseId);
        IEnumerable<Grade> GetByFacultyId(int facultyId);
        Grade? GetByStudentAndCourse(int studentId, int courseId);
        void Add(Grade grade);
        void Update(Grade grade);
        void Delete(int id);
    }
}

