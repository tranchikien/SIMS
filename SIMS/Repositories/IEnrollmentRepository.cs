using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface IEnrollmentRepository
    {
        IEnumerable<Enrollment> GetAll();
        Enrollment? GetById(int id);
        IEnumerable<Enrollment> GetByStudentId(int studentId);
        IEnumerable<Enrollment> GetByCourseId(int courseId);
        IEnumerable<Enrollment> GetByFacultyId(int facultyId);
        void Add(Enrollment enrollment);
        void Update(Enrollment enrollment);
        void Delete(int id);
    }
}

