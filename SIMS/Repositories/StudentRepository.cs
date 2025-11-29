using Microsoft.EntityFrameworkCore;
using SIMS.Data;
using SIMS.Models;
using System.Collections.Generic;
using System.Linq;

namespace SIMS.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SIMSDbContext _context;

        public StudentRepository(SIMSDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Student> GetAll()
        {
            return _context.Students.ToList();
        }

        public Student? GetById(int id)
        {
            return _context.Students.FirstOrDefault(s => s.Id == id);
        }

        public Student? GetByStudentId(string studentId)
        {
            return _context.Students.FirstOrDefault(s => s.StudentId == studentId);
        }

        public Student? GetByEmail(string email)
        {
            return _context.Students.FirstOrDefault(s => s.Email == email);
        }

        public void Add(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
        }

        public void Update(Student student)
        {
            _context.Students.Update(student);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            if (student != null)
            {
                _context.Students.Remove(student);
                _context.SaveChanges();
            }
        }

        public int GetCount()
        {
            return _context.Students.Count();
        }
    }
}

