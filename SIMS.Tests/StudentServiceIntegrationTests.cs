using System.Linq;
using Microsoft.EntityFrameworkCore;
using SIMS.Data;
using SIMS.Models;
using SIMS.Repositories;
using SIMS.Services;
using Xunit;

namespace SIMS.Tests;

/// <summary>
/// Kiểm thử tích hợp (Integration Testing):
/// - Kết hợp StudentService + Repository + DbContext (InMemory).
/// - Không chạm tới giao diện hay HTTP, chỉ test ở tầng service & database.
/// </summary>
public class StudentServiceIntegrationTests
{
    private SIMSDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SIMSDbContext>()
            .UseInMemoryDatabase(databaseName: "SIMS_Integration_Students_" + System.Guid.NewGuid())
            .Options;

        return new SIMSDbContext(options);
    }

    [Fact]
    public void CreateStudent_PersistsStudentAndUser_InDatabase()
    {
        using var context = CreateInMemoryContext();

        var studentRepo = new StudentRepository(context);
        var userRepo = new UserRepository(context);
        var enrollmentRepo = new EnrollmentRepository(context);
        var service = new StudentService(studentRepo, userRepo, enrollmentRepo);

        var model = new StudentViewModel
        {
            FullName = "Integration Student",
            Email = "integration@student.com",
            StudentId = "S100",
            Program = "CS",
            Password = "123456"
        };

        // Act
        var (success, _) = service.CreateStudent(model);

        // Assert
        Assert.True(success);
        Assert.Equal(1, context.Students.Count());
        Assert.Equal(1, context.Users.Count());

        var student = context.Students.Single();
        var user = context.Users.Single();

        Assert.Equal(student.Id, user.ReferenceId);
        Assert.Equal("Student", user.Role);
    }

    [Fact]
    public void UpdateStudent_ChangesData_InAllTables()
    {
        using var context = CreateInMemoryContext();

        // Seed
        var student = new Student
        {
            StudentId = "S200",
            FullName = "Old Name",
            Email = "old@student.com",
            Program = "IT",
            Status = "Active"
        };
        context.Students.Add(student);
        context.SaveChanges();

        var user = new User
        {
            Username = "S200",
            Password = "old",
            FullName = "Old Name",
            Email = "old@student.com",
            Role = "Student",
            ReferenceId = student.Id,
            Status = "Active"
        };
        context.Users.Add(user);
        context.SaveChanges();

        var studentRepo = new StudentRepository(context);
        var userRepo = new UserRepository(context);
        var enrollmentRepo = new EnrollmentRepository(context);
        var service = new StudentService(studentRepo, userRepo, enrollmentRepo);

        var model = new StudentEditViewModel
        {
            FullName = "New Name",
            Email = "new@student.com",
            StudentId = "S200",
            Program = "SE",
            Status = "Inactive",
            Password = "newpass"
        };

        // Act
        var (success, _) = service.UpdateStudent(student.Id, model);

        // Assert
        Assert.True(success);

        var updatedStudent = context.Students.Single();
        var updatedUser = context.Users.Single();

        Assert.Equal("New Name", updatedStudent.FullName);
        Assert.Equal("new@student.com", updatedStudent.Email);
        Assert.Equal("SE", updatedStudent.Program);
        Assert.Equal("Inactive", updatedStudent.Status);

        Assert.Equal("New Name", updatedUser.FullName);
        Assert.Equal("new@student.com", updatedUser.Email);
        Assert.Equal("S200", updatedUser.Username);
        Assert.Equal("Inactive", updatedUser.Status);
        Assert.Equal("newpass", updatedUser.Password);
    }

    [Fact]
    public void DeleteStudent_RemovesStudentUserAndEnrollments_FromDatabase()
    {
        using var context = CreateInMemoryContext();

        var student = new Student
        {
            StudentId = "S300",
            FullName = "Student 300",
            Email = "s300@student.com",
            Program = "IT",
            Status = "Active"
        };
        context.Students.Add(student);
        context.SaveChanges();

        var user = new User
        {
            Username = "S300",
            Password = "pass",
            FullName = "Student 300",
            Email = "s300@student.com",
            Role = "Student",
            ReferenceId = student.Id,
            Status = "Active"
        };
        context.Users.Add(user);

        context.Enrollments.Add(new Enrollment
        {
            StudentId = student.Id,
            CourseId = 1,
            Status = "Enrolled"
        });
        context.SaveChanges();

        var studentRepo = new StudentRepository(context);
        var userRepo = new UserRepository(context);
        var enrollmentRepo = new EnrollmentRepository(context);
        var service = new StudentService(studentRepo, userRepo, enrollmentRepo);

        // Act
        var result = service.DeleteStudent(student.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(context.Students);
        Assert.Empty(context.Users);
        Assert.Empty(context.Enrollments);
    }
}


