using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetAll();
        Student? GetById(int id);
        Student? GetByStudentId(string studentId);
        Student? GetByEmail(string email);
        void Add(Student student);
        void Update(Student student);
        void Delete(int id);
        int GetCount();
    }
}

