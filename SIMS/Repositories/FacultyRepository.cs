using SIMS.Data;
using SIMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Repositories
{
    public class FacultyRepository : IFacultyRepository
    {
        private readonly SIMSDbContext _context;

        public FacultyRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Faculty> GetAll()
        {
            return _context.Faculties.ToList();
        }

        public Faculty? GetById(int id)
        {
            return _context.Faculties.FirstOrDefault(f => f.Id == id);
        }

        public Faculty? GetByFacultyId(string facultyId)
        {
            return _context.Faculties.FirstOrDefault(f => f.FacultyId == facultyId);
        }

        public Faculty? GetByEmail(string email)
        {
            return _context.Faculties.FirstOrDefault(f => f.Email == email);
        }

        public void Add(Faculty faculty)
        {
            _context.Faculties.Add(faculty);
            _context.SaveChanges();
        }

        public void Update(Faculty faculty)
        {
            _context.Faculties.Update(faculty);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var faculty = _context.Faculties.FirstOrDefault(f => f.Id == id);
            if (faculty != null)
            {
                _context.Faculties.Remove(faculty);
                _context.SaveChanges();
            }
        }

        public int GetCount()
        {
            return _context.Faculties.Count();
        }
    }
}

