using SIMS.Models;
using System.Collections.Generic;

namespace SIMS.Repositories
{
    public interface ICourseRepository
    {
        IEnumerable<Course> GetAll();
        Course? GetById(int id);
        void Add(Course course);
        void Update(Course course);
        void Delete(int id);
        int GetCount();
    }
}

