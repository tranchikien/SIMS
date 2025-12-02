using System;
using System.Collections.Generic;
using System.Linq;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using Xunit;

namespace SIMS.Tests;

public class AuthenticationServiceTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeStudentRepository _studentRepository = new();
    private readonly FakeFacultyRepository _facultyRepository = new();

    private AuthenticationService CreateService()
    {
        return new AuthenticationService(_userRepository, _studentRepository, _facultyRepository);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        var service = CreateService();

        // Act
        var (success, role, userInfo) = service.Authenticate("unknown", "password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenPasswordDoesNotMatch()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 1,
            Username = "student1",
            Password = "correct-password",
            FullName = "Student One",
            Email = "student1@example.com",
            Role = "Student",
            ReferenceId = 1,
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("student1", "wrong-password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    [Fact]
    public void Authenticate_ReturnsStudentInfo_WhenValidStudent()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 1,
            Username = "student1",
            Password = "password",
            FullName = "Student One",
            Email = "student1@example.com",
            Role = "Student",
            ReferenceId = 10,
            Status = "Active"
        });

        _studentRepository.Students.Add(new Student
        {
            Id = 10,
            StudentId = "S001",
            FullName = "Student One",
            Email = "student1@example.com",
            Program = "IT",
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("student1", "password");

        // Assert
        Assert.True(success);
        Assert.Equal("Student", role);
        Assert.NotNull(userInfo);
        Assert.Equal(10, userInfo!.Id);
        Assert.Equal("Student One", userInfo.FullName);
    }

    [Fact]
    public void Authenticate_ReturnsFacultyInfo_WhenValidFaculty()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 2,
            Username = "faculty1",
            Password = "password",
            FullName = "Faculty One",
            Email = "faculty1@example.com",
            Role = "Faculty",
            ReferenceId = 20,
            Status = "Active"
        });

        _facultyRepository.Faculties.Add(new Faculty
        {
            Id = 20,
            FacultyId = "F001",
            FullName = "Faculty One",
            Email = "faculty1@example.com",
            Department = "CS",
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("faculty1", "password");

        // Assert
        Assert.True(success);
        Assert.Equal("Faculty", role);
        Assert.NotNull(userInfo);
        Assert.Equal(20, userInfo!.Id);
        Assert.Equal("Faculty One", userInfo.FullName);
    }

    [Fact]
    public void Authenticate_ReturnsAdminInfo_WhenValidAdmin()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 3,
            Username = "admin",
            Password = "password",
            FullName = "Admin User",
            Email = "admin@example.com",
            Role = "Admin",
            ReferenceId = null,
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("admin", "password");

        // Assert
        Assert.True(success);
        Assert.Equal("Admin", role);
        Assert.NotNull(userInfo);
        Assert.Equal(3, userInfo!.Id);
        Assert.Equal("Admin User", userInfo.FullName);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenUserIsInactive()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 4,
            Username = "inactive",
            Password = "password",
            FullName = "Inactive User",
            Email = "inactive@example.com",
            Role = "Student",
            ReferenceId = 10,
            Status = "Inactive"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("inactive", "password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenStudentReferenceIdIsMissing()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 5,
            Username = "student-no-ref",
            Password = "password",
            FullName = "Student No Ref",
            Email = "noref@student.com",
            Role = "Student",
            ReferenceId = null,
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("student-no-ref", "password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenStudentNotFoundForReferenceId()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 6,
            Username = "student-missing",
            Password = "password",
            FullName = "Student Missing",
            Email = "missing@student.com",
            Role = "Student",
            ReferenceId = 999, // no corresponding student
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("student-missing", "password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenFacultyNotFoundForReferenceId()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 7,
            Username = "faculty-missing",
            Password = "password",
            FullName = "Faculty Missing",
            Email = "missing@faculty.com",
            Role = "Faculty",
            ReferenceId = 999, // no corresponding faculty
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("faculty-missing", "password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    [Fact]
    public void Authenticate_UsesEmailForLookup_WhenUsernameIsEmail()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 8,
            Username = "student8",
            Password = "password",
            FullName = "Student Email",
            Email = "student8@example.com",
            Role = "Student",
            ReferenceId = 80,
            Status = "Active"
        });

        _studentRepository.Students.Add(new Student
        {
            Id = 80,
            StudentId = "S080",
            FullName = "Student Email",
            Email = "student8@example.com",
            Program = "IT",
            Status = "Active"
        });

        // Act - login bằng email thay vì username
        var (success, role, userInfo) = service.Authenticate("student8@example.com", "password");

        // Assert
        Assert.True(success);
        Assert.Equal("Student", role);
        Assert.NotNull(userInfo);
        Assert.Equal(80, userInfo!.Id);
    }

    [Fact]
    public void Authenticate_ReturnsFalse_WhenRoleIsUnknown()
    {
        // Arrange
        var service = CreateService();
        _userRepository.Users.Add(new User
        {
            Id = 9,
            Username = "unknown-role",
            Password = "password",
            FullName = "Unknown Role",
            Email = "unknown@example.com",
            Role = "Manager", // không thuộc Student/Faculty/Admin
            ReferenceId = null,
            Status = "Active"
        });

        // Act
        var (success, role, userInfo) = service.Authenticate("unknown-role", "password");

        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, role);
        Assert.Null(userInfo);
    }

    #region Fake repositories

    private sealed class FakeUserRepository : IUserRepository
    {
        public List<User> Users { get; } = new();

        public User? GetByUsernameOrEmail(string usernameOrEmail)
        {
            return Users.FirstOrDefault(u =>
                string.Equals(u.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(u.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase));
        }

        public User? GetById(int id) => Users.FirstOrDefault(u => u.Id == id);

        public User? GetByUsername(string username) =>
            Users.FirstOrDefault(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

        public User? GetByEmail(string email) =>
            Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        public IEnumerable<User> GetAll() => Users;

        public IEnumerable<User> GetByRole(string role) =>
            Users.Where(u => string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase));

        public User? GetByReferenceId(int referenceId, string role) =>
            Users.FirstOrDefault(u => u.ReferenceId == referenceId &&
                                      string.Equals(u.Role, role, StringComparison.OrdinalIgnoreCase));

        public void Add(User user) => Users.Add(user);

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

    private sealed class FakeStudentRepository : IStudentRepository
    {
        public List<Student> Students { get; } = new();

        public IEnumerable<Student> GetAll() => Students;

        public Student? GetById(int id) => Students.FirstOrDefault(s => s.Id == id);

        public Student? GetByStudentId(string studentId) =>
            Students.FirstOrDefault(s => string.Equals(s.StudentId, studentId, StringComparison.OrdinalIgnoreCase));

        public Student? GetByEmail(string email) =>
            Students.FirstOrDefault(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));

        public void Add(Student student) => Students.Add(student);

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

    private sealed class FakeFacultyRepository : IFacultyRepository
    {
        public List<Faculty> Faculties { get; } = new();

        public IEnumerable<Faculty> GetAll() => Faculties;

        public Faculty? GetById(int id) => Faculties.FirstOrDefault(f => f.Id == id);

        public Faculty? GetByFacultyId(string facultyId) =>
            Faculties.FirstOrDefault(f => string.Equals(f.FacultyId, facultyId, StringComparison.OrdinalIgnoreCase));

        public Faculty? GetByEmail(string email) =>
            Faculties.FirstOrDefault(f => string.Equals(f.Email, email, StringComparison.OrdinalIgnoreCase));

        public void Add(Faculty faculty) => Faculties.Add(faculty);

        public void Update(Faculty faculty)
        {
            var existing = GetById(faculty.Id);
            if (existing == null) return;
            Faculties.Remove(existing);
            Faculties.Add(faculty);
        }

        public void Delete(int id)
        {
            var existing = GetById(id);
            if (existing != null)
            {
                Faculties.Remove(existing);
            }
        }

        public int GetCount() => Faculties.Count;
    }

    #endregion
}


