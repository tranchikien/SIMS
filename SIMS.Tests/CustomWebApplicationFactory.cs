using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SIMS.Data;
using SIMS.Models;

namespace SIMS.Tests;

/// <summary>
/// WebApplicationFactory dùng môi trường "Testing" và seed dữ liệu cho E2E tests.
/// DbContext sẽ dùng InMemory do cấu hình trong Program.cs khi Environment = "Testing".
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Build provider và seed dữ liệu
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<SIMSDbContext>();

            db.Database.EnsureCreated();

            if (!db.Users.Any())
            {
                // Seed student
                var student = new Student
                {
                    StudentId = "S999",
                    FullName = "E2E Student",
                    Email = "e2e@student.com",
                    Program = "IT",
                    Status = "Active"
                };
                db.Students.Add(student);
                db.SaveChanges();

                db.Users.Add(new User
                {
                    Username = "S999",
                    Password = "password",
                    FullName = student.FullName,
                    Email = student.Email,
                    Role = "Student",
                    ReferenceId = student.Id,
                    Status = "Active"
                });

                // Seed admin
                db.Users.Add(new User
                {
                    Username = "admin",
                    Password = "admin123",
                    FullName = "Admin User",
                    Email = "admin@example.com",
                    Role = "Admin",
                    ReferenceId = null,
                    Status = "Active"
                });

                db.SaveChanges();
            }
        });
    }
}



