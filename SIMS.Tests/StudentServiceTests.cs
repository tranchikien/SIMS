using System;
using System.Collections.Generic;
using System.Linq;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using Xunit;

namespace SIMS.Tests;

/// <summary>
/// Kiểm thử đơn vị (Unit Testing) cho StudentService:
/// - Chỉ tập trung vào logic của StudentService
/// - Các phụ thuộc (repository) được fake bằng in-memory list.
/// </summary>
public class StudentServiceTests
{
    private readonly FakeStudentRepository _studentRepository = new();
    private readonly FakeUserRepository _userRepository = new();

    private StudentService CreateService()
    {
        return new StudentService(_studentRepository, _userRepository, new FakeEnrollmentRepository());
    }

    [Fact]
    public void CreateStudent_Succeeds_WhenDataIsValidAndUnique()
    {
        // Arrange
        var service = CreateService();
        var model = new StudentViewModel
        {
            FullName = "Student One",
            Email = "s1@example.com",
            StudentId = "S001",
            Program = "IT",
            Password = "pass123"
        };

        // Act
        var (success, error) = service.CreateStudent(model);

        // Assert
        Assert.True(success);
        Assert.Null(error);

        Assert.Single(_studentRepository.Students);
        Assert.Single(_userRepository.Users);

        var student = _studentRepository.Students.Single();
        var user = _userRepository.Users.Single();

        Assert.Equal(student.Id, user.ReferenceId);
        Assert.Equal("Student", user.Role);
        Assert.Equal(model.StudentId, user.Username);
    }

    [Fact]
    public void CreateStudent_Fails_WhenStudentIdIsDuplicate()
    {
        // Arrange
        var service = CreateService();
        _studentRepository.Students.Add(new Student
        {
            Id = 1,
            StudentId = "S001",
            FullName = "Existing Student",
            Email = "exist@example.com",
            Program = "IT",
            Status = "Active"
        });

        var model = new StudentViewModel
        {
            FullName = "New Student",
            Email = "new@example.com",
            StudentId = "S001", // trùng
            Program = "IT",
            Password = "pass123"
        };

        // Act
        var (success, error) = service.CreateStudent(model);

        // Assert
        Assert.False(success);
        Assert.Equal("Student ID already exists.", error);
    }

    [Fact]
    public void UpdateStudent_UpdatesStudentAndUser()
    {
        // Arrange
        var service = CreateService();

        var student = new Student
        {
            Id = 1,
            StudentId = "S001",
            FullName = "Old Name",
            Email = "old@example.com",
            Program = "IT",
            Status = "Active"
        };
        _studentRepository.Students.Add(student);

        var user = new User
        {
            Id = 10,
            Username = "S001",
            Password = "old",
            FullName = "Old Name",
            Email = "old@example.com",
            Role = "Student",
            ReferenceId = 1,
            Status = "Active"
        };
        _userRepository.Users.Add(user);

        var model = new StudentEditViewModel
        {
            FullName = "New Name",
            Email = "new@example.com",
            StudentId = "S001",
            Program = "CS",
            Status = "Inactive",
            Password = "newpass"
        };

        // Act
        var (success, error) = service.UpdateStudent(1, model);

        // Assert
        Assert.True(success);
        Assert.Null(error);

        var updatedStudent = _studentRepository.Students.Single();
        var updatedUser = _userRepository.Users.Single();

        Assert.Equal("New Name", updatedStudent.FullName);
        Assert.Equal("new@example.com", updatedStudent.Email);
        Assert.Equal("CS", updatedStudent.Program);
        Assert.Equal("Inactive", updatedStudent.Status);

        Assert.Equal("New Name", updatedUser.FullName);
        Assert.Equal("new@example.com", updatedUser.Email);
        Assert.Equal("S001", updatedUser.Username);
        Assert.Equal("Inactive", updatedUser.Status);
        Assert.Equal("newpass", updatedUser.Password);
    }

    [Fact]
    public void DeleteStudent_DeletesStudentUserAndEnrollments()
    {
        // Arrange
        var enrollRepo = new FakeEnrollmentRepository();
        var service = new StudentService(_studentRepository, _userRepository, enrollRepo);

        _studentRepository.Students.Add(new Student
        {
            Id = 1,
            StudentId = "S001",
            FullName = "Student One",
            Email = "s1@example.com",
            Program = "IT",
            Status = "Active"
        });

        _userRepository.Users.Add(new User
        {
            Id = 10,
            Username = "S001",
            Password = "pass",
            FullName = "Student One",
            Email = "s1@example.com",
            Role = "Student",
            ReferenceId = 1,
            Status = "Active"
        });

        enrollRepo.Enrollments.Add(new Enrollment
        {
            Id = 100,
            StudentId = 1,
            CourseId = 1,
            Status = "Enrolled"
        });

        // Act
        var result = service.DeleteStudent(1);

        // Assert
        Assert.True(result);
        Assert.Empty(_studentRepository.Students);
        Assert.Empty(_userRepository.Users);
        Assert.Empty(enrollRepo.Enrollments);
    }

    #region Fake Repositories

    private sealed class FakeStudentRepository : IStudentRepository
    {
        public List<Student> Students { get; } = new();

        public IEnumerable<Student> GetAll() => Students;

        public Student? GetById(int id) => Students.FirstOrDefault(s => s.Id == id);

        public Student? GetByStudentId(string studentId) =>
            Students.FirstOrDefault(s => string.Equals(s.StudentId, studentId, StringComparison.OrdinalIgnoreCase));

        public Student? GetByEmail(string email) =>
            Students.FirstOrDefault(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));

        public void Add(Student student)
        {
            if (student.Id == 0)
            {
                student.Id = Students.Count == 0 ? 1 : Students.Max(s => s.Id) + 1;
            }
            Students.Add(student);
        }

        public void Update(Student student)
        {
            var existing = GetById(student.Id);
            if (existing == null) return;
            Students.Remove(existing);
            Students.Add(student);
        }

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                Students.Remove(existing);
            }
        }

        public int GetCount() => Students.Count;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = new();

        public User? GetById(int id) => Users.FirstOrDefault(u => u.Id == id);

        public User? GetByUsername(string username) =>
            Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        public User? GetByEmail(string email) =>
            Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        public User? GetByUsernameOrEmail(string usernameOrEmail)
        {
            return Users.FirstOrDefault(u =>
                string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<User> GetAll() => Users;

        public IEnumerable<User> GetByRole(string role) =>
            Users.Where(u => string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase));

        public User? GetByReferenceId(int referenceId, string role) =>
            Users.FirstOrDefault(u => u.ReferenceId == referenceId &&
                                      string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase));

        public void Add(User user)
        {
            if (user.Id == 0)
            {
                user.Id = Users.Count == 0 ? 1 : Users.Max(u => u.Id) + 1;
            }
            Users.Add(user);
        }

        public void Update(User user)
        {
            var existing = GetById(user.Id);
            if (existing == null) return;
            Users.Remove(existing);
            Users.Add(user);
        }

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                Users.Remove(existing);
            }
        }

        public int GetCount() => Users.Count;
    }

    private sealed class FakeEnrollmentRepository : IEnrollmentRepository
    {
        public List<Enrollment> Enrollments { get; } = new();

        public IEnumerable<Enrollment> GetAll() => Enrollments;

        public Enrollment? GetById(int id) => Enrollments.FirstOrDefault(e => e.Id == id);

        public IEnumerable<Enrollment> GetByStudentId(int studentId) =>
            Enrollments.Where(e => e.StudentId == studentId);

        public IEnumerable<Enrollment> GetByCourseId(int courseId) =>
            Enrollments.Where(e => e.CourseId == courseId);

        public IEnumerable<Enrollment> GetByFacultyId(int facultyId) =>
            Enrollments.Where(e => e.FacultyId == facultyId);

        public void Add(Enrollment enrollment)
        {
            if (enrollment.Id == 0)
            {
                enrollment.Id = Enrollments.Count == 0 ? 1 : Enrollments.Max(e => e.Id) + 1;
            }
            Enrollments.Add(enrollment);
        }

        public void Update(Enrollment enrollment)
        {
            var existing = GetById(enrollment.Id);
            if (existing == null) return;
            Enrollments.Remove(existing);
            Enrollments.Add(enrollment);
        }

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                Enrollments.Remove(existing);
            }
        }

        public int GetCount() => Enrollments.Count;
    }

    #endregion
}


