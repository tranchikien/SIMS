using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface IFacultyRepository
    {
        IEnumerable<Faculty> GetAll();
        Faculty? GetById(int id);
        Faculty? GetByFacultyId(string facultyId);
        Faculty? GetByEmail(string email);
        void Add(Faculty faculty);
        void Update(Faculty faculty);
        void Delete(int id);
        int GetCount();
    }
}

