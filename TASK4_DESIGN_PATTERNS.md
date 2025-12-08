# TASK 4: DESIGN PATTERNS TRONG DỰ ÁN SIMS

## TỔNG QUAN

Dự án SIMS áp dụng nhiều design patterns quan trọng để đảm bảo code dễ maintain, testable và tuân thủ SOLID principles. Các patterns được sử dụng bao gồm:

1. **Repository Pattern** - Tách biệt data access logic
2. **Service Pattern** - Encapsulate business logic
3. **Dependency Injection Pattern** - Loose coupling và testability
4. **Factory Pattern** - Tạo objects phức tạp
5. **Template Method Pattern** - Định nghĩa skeleton của algorithm

---

## 1. REPOSITORY PATTERN

### Mục đích:
Repository Pattern tách biệt logic truy cập dữ liệu khỏi business logic, giúp code dễ test và maintain hơn.

### Cách triển khai trong dự án:

#### 1.1. Định nghĩa Interface (Abstraction)

```csharp
// SIMS/Repositories/IStudentRepository.cs
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
```

**Giải thích:**
- Interface định nghĩa **contract** cho data access operations
- Không phụ thuộc vào implementation cụ thể (SQL Server, InMemory, MongoDB...)
- Tuân thủ **Dependency Inversion Principle (DIP)** - depend on abstraction, not concretion

#### 1.2. Implementation với Entity Framework Core

```csharp
// SIMS/Repositories/StudentRepository.cs
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

        // Constructor injection (Dependency Injection Pattern)
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
```

**Giải thích:**
- `StudentRepository` implement `IStudentRepository`
- Sử dụng `SIMSDbContext` để truy cập database
- Mỗi method đại diện cho một operation cụ thể (CRUD)
- `SaveChanges()` được gọi trong mỗi write operation để persist changes

#### 1.3. Sử dụng trong Service Layer

```csharp
// SIMS/Services/StudentService.cs
namespace SIMS.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        // Dependency Injection - nhận interface, không phải concrete class
        public StudentService(
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public IEnumerable<Student> GetAllStudents(string? searchString = null)
        {
            var students = _studentRepository.GetAll(); // Sử dụng repository

            if (!string.IsNullOrEmpty(searchString))
            {
                students = students.Where(s =>
                    s.FullName.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase));
            }

            return students;
        }
    }
}
```

**Giải thích:**
- Service chỉ phụ thuộc vào **interface** (`IStudentRepository`), không phụ thuộc vào implementation cụ thể
- Dễ dàng thay thế implementation (ví dụ: fake repository cho unit test)
- Business logic tách biệt hoàn toàn khỏi data access logic

#### 1.4. Đăng ký trong Dependency Injection Container

```csharp
// SIMS/Program.cs
// Register Repositories (SOLID: Dependency Inversion)
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
```

**Giải thích:**
- Đăng ký mapping giữa interface và implementation
- `AddScoped`: Một instance được tạo cho mỗi HTTP request
- Khi có class cần `IStudentRepository`, DI container sẽ tự động inject `StudentRepository`

### Lợi ích của Repository Pattern:

✅ **Testability**: Dễ dàng tạo fake repository cho unit test
```csharp
// SIMS.Tests/StudentServiceTests.cs
private sealed class FakeStudentRepository : IStudentRepository
{
    public List<Student> Students { get; } = new();
    public IEnumerable<Student> GetAll() => Students;
    // ... implement các methods khác
}
```

✅ **Flexibility**: Có thể thay đổi data source (SQL Server → MongoDB) mà không ảnh hưởng business logic

✅ **Single Responsibility**: Repository chỉ lo việc data access, Service lo business logic

✅ **Maintainability**: Code dễ đọc và maintain hơn

---

## 2. SERVICE PATTERN

### Mục đích:
Service Pattern encapsulate business logic vào các service classes, tách biệt khỏi presentation layer (Controller) và data access layer (Repository).

### Cách triển khai trong dự án:

#### 2.1. Định nghĩa Service Interface

```csharp
// SIMS/Services/IStudentService.cs
namespace SIMS.Services
{
    /// <summary>
    /// Service for student business logic (SOLID: Single Responsibility)
    /// </summary>
    public interface IStudentService
    {
        IEnumerable<Student> GetAllStudents(string? searchString = null);
        (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model);
        (bool Success, string? ErrorMessage) UpdateStudent(int id, StudentEditViewModel model);
        bool DeleteStudent(int id);
        bool IsStudentIdUnique(string studentId, int? excludeId = null);
        bool IsEmailUnique(string email, int? excludeId = null);
        Student? GetStudentById(int id);
    }
}
```

**Giải thích:**
- Interface định nghĩa các business operations
- Tuân thủ **Interface Segregation Principle (ISP)** - interface chỉ chứa methods cần thiết
- Return types rõ ràng: `(bool Success, string? ErrorMessage)` cho operations có thể fail

#### 2.2. Implementation với Business Logic

```csharp
// SIMS/Services/StudentService.cs
namespace SIMS.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IEnrollmentRepository _enrollmentRepository;

        public StudentService(
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            IEnrollmentRepository enrollmentRepository)
        {
            _studentRepository = studentRepository;
            _userRepository = userRepository;
            _enrollmentRepository = enrollmentRepository;
        }

        public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
        {
            // Business Rule 1: Validate uniqueness
            if (!IsStudentIdUnique(model.StudentId))
            {
                return (false, "Student ID already exists.");
            }

            if (!IsEmailUnique(model.Email))
            {
                return (false, "Email already exists.");
            }

            // Business Rule 2: Create Student entity
            var student = new Student
            {
                StudentId = model.StudentId,
                FullName = model.FullName,
                Email = model.Email,
                Program = model.Program,
                Status = "Active"
            };
            _studentRepository.Add(student);

            // Business Rule 3: Create corresponding User account
            var user = new User
            {
                Username = model.StudentId,
                Password = model.Password,
                FullName = model.FullName,
                Email = model.Email,
                Role = "Student",
                ReferenceId = student.Id,
                Status = "Active"
            };
            _userRepository.Add(user);

            return (true, null);
        }

        public bool DeleteStudent(int id)
        {
            var student = _studentRepository.GetById(id);
            if (student == null)
            {
                return false;
            }

            // Business Rule: Delete related entities
            var user = _userRepository.GetByReferenceId(student.Id, "Student");
            if (user != null)
            {
                _userRepository.Delete(user.Id);
            }

            // Delete enrollments
            var enrollments = _enrollmentRepository.GetAll()
                .Where(e => e.StudentId == student.Id);
            foreach (var enrollment in enrollments)
            {
                _enrollmentRepository.Delete(enrollment.Id);
            }

            _studentRepository.Delete(id);
            return true;
        }

        public bool IsStudentIdUnique(string studentId, int? excludeId = null)
        {
            var existing = _studentRepository.GetByStudentId(studentId);
            return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
        }
    }
}
```

**Giải thích:**
- Service chứa **business logic**: validation, business rules, orchestration
- Service **orchestrate** nhiều repositories để hoàn thành một business operation
- Tuân thủ **Single Responsibility Principle (SRP)** - mỗi service chỉ lo một domain (Student, Faculty, Course...)

#### 2.3. Sử dụng trong Controller

```csharp
// SIMS/Controllers/StudentController.cs
namespace SIMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly IAuthorizationService _authorizationService;

        public StudentController(
            IStudentService studentService,
            IAuthorizationService authorizationService)
        {
            _studentService = studentService;
            _authorizationService = authorizationService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    // Controller chỉ gọi service, không chứa business logic
                    var (success, errorMessage) = _studentService.CreateStudent(model);
                    
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Student added successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    
                    ModelState.AddModelError(string.Empty, errorMessage ?? "An error occurred.");
                }

                return View(model);
            });
        }
    }
}
```

**Giải thích:**
- Controller chỉ lo **HTTP concerns**: routing, model binding, view rendering
- Business logic được delegate cho Service
- Controller trở nên **thin** và dễ test

### Lợi ích của Service Pattern:

✅ **Separation of Concerns**: Business logic tách biệt khỏi presentation và data access

✅ **Reusability**: Service có thể được sử dụng bởi nhiều controllers hoặc API endpoints

✅ **Testability**: Dễ test business logic độc lập với HTTP context

✅ **Maintainability**: Thay đổi business logic không ảnh hưởng đến Controller

---

## 3. DEPENDENCY INJECTION PATTERN

### Mục đích:
Dependency Injection (DI) giảm coupling giữa các classes bằng cách inject dependencies từ bên ngoài thay vì tạo bên trong class.

### Cách triển khai trong dự án:

#### 3.1. Constructor Injection trong Service

```csharp
// SIMS/Services/AuthenticationService.cs
namespace SIMS.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IFacultyRepository _facultyRepository;

        // Constructor Injection: Dependencies được inject qua constructor
        public AuthenticationService(
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            IFacultyRepository facultyRepository)
        {
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _facultyRepository = facultyRepository;
        }

        public (bool Success, string Role, UserInfo? UserInfo) Authenticate(string username, string password)
        {
            var user = _userRepository.GetByUsernameOrEmail(username);
            
            if (user != null && user.Password == password && user.Status == "Active")
            {
                if (user.Role == "Student" && user.ReferenceId.HasValue)
                {
                    var student = _studentRepository.GetById(user.ReferenceId.Value);
                    if (student != null)
                    {
                        return (true, "Student", new UserInfo { Id = user.ReferenceId.Value, FullName = student.FullName });
                    }
                }
                // ... Faculty và Admin logic
            }

            return (false, string.Empty, null);
        }
    }
}
```

**Giải thích:**
- Dependencies được khai báo là `readonly` fields
- Dependencies được inject qua constructor parameters
- Class không tự tạo dependencies → **loose coupling**

#### 3.2. Đăng ký trong DI Container (Program.cs)

```csharp
// SIMS/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register Repositories (SOLID: Dependency Inversion)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();

// Register Services (SOLID: Single Responsibility)
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IFacultyService, FacultyService>();
```

**Giải thích:**
- `AddScoped`: Một instance được tạo cho mỗi HTTP request (scoped lifetime)
- Khi `AuthenticationService` được yêu cầu, DI container sẽ:
  1. Tạo `UserRepository`
  2. Tạo `StudentRepository`
  3. Tạo `FacultyRepository`
  4. Inject chúng vào constructor của `AuthenticationService`
  5. Trả về instance của `AuthenticationService`

#### 3.3. Sử dụng trong Controller

```csharp
// SIMS/Controllers/LoginController.cs
namespace SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        // DI Container tự động inject IAuthenticationService
        public LoginController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            var (success, role, userInfo) = _authenticationService.Authenticate(model.Username, model.Password);
            
            if (success)
            {
                _authenticationService.SetSession(HttpContext, role, model.Username, userInfo);
                // Redirect based on role...
            }
            
            return View(model);
        }
    }
}
```

**Giải thích:**
- Controller không cần biết cách tạo `AuthenticationService`
- DI container tự động resolve và inject dependencies
- Controller chỉ cần khai báo dependencies trong constructor

### Lợi ích của Dependency Injection Pattern:

✅ **Loose Coupling**: Classes không phụ thuộc vào concrete implementations

✅ **Testability**: Dễ dàng inject fake dependencies cho unit test
```csharp
// Unit test có thể inject fake repository
var fakeUserRepo = new FakeUserRepository();
var service = new AuthenticationService(fakeUserRepo, ...);
```

✅ **Flexibility**: Dễ dàng thay đổi implementation mà không sửa code của consumer

✅ **Single Responsibility**: Mỗi class chỉ lo việc của mình, không lo việc tạo dependencies

---

## 4. FACTORY PATTERN

### Mục đích:
Factory Pattern cung cấp một cách để tạo objects mà không cần specify exact class của object sẽ được tạo.

### Cách triển khai trong dự án:

#### 4.1. CustomWebApplicationFactory cho E2E Testing

```csharp
// SIMS.Tests/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SIMS.Data;
using SIMS.Models;

namespace SIMS.Tests;

/// <summary>
/// Factory để tạo test web application với configuration đặc biệt cho testing
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Testing" để Program.cs sử dụng InMemory database
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Build service provider để access DbContext
            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<SIMSDbContext>();

            // Ensure database is created
            db.Database.EnsureCreated();

            // Seed test data
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

                // Seed user for student
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

                // Seed admin user
                db.Users.Add(new User
                {
                    Username = "admin",
                    Password = "admin",
                    FullName = "Admin User",
                    Email = "admin@example.com",
                    Role = "Admin",
                    Status = "Active"
                });

                db.SaveChanges();
            }
        });
    }
}
```

**Giải thích:**
- `WebApplicationFactory<T>` là factory pattern của ASP.NET Core
- `CustomWebApplicationFactory` customize cách tạo test application:
  - Set environment = "Testing" → Program.cs sẽ dùng InMemory DB
  - Seed test data vào database
- Mỗi test có thể tạo một instance mới với `_factory.CreateClient()`

#### 4.2. Sử dụng trong E2E Tests

```csharp
// SIMS.Tests/LoginE2ETests.cs
namespace SIMS.Tests;

public class LoginE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public LoginE2ETests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(); // Factory tạo HttpClient
    }

    [Fact]
    public async Task Post_Login_ValidStudent_RedirectsToStudentDashboard()
    {
        // Arrange
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Username", "S999"),
            new KeyValuePair<string, string>("Password", "password")
        });

        // Act
        var response = await _client.PostAsync("/Login/Index", content);

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/StudentDashboard", response.Headers.Location?.ToString());
    }
}
```

**Giải thích:**
- `IClassFixture<CustomWebApplicationFactory>`: Factory được tạo một lần và share cho tất cả tests trong class
- `_factory.CreateClient()`: Factory tạo HttpClient với configuration đặc biệt (InMemory DB, seeded data)

### Lợi ích của Factory Pattern:

✅ **Encapsulation**: Logic tạo object phức tạp được encapsulate trong factory

✅ **Flexibility**: Có thể thay đổi cách tạo object mà không ảnh hưởng client code

✅ **Testability**: Dễ dàng tạo test objects với configuration đặc biệt

✅ **Reusability**: Factory có thể được reuse cho nhiều tests

---

## 5. TEMPLATE METHOD PATTERN

### Mục đích:
Template Method Pattern định nghĩa skeleton của algorithm trong base class, để các subclasses override một số steps mà không thay đổi structure của algorithm.

### Cách triển khai trong dự án:

#### 5.1. AuthorizationService với Template Method

```csharp
// SIMS/Services/AuthorizationService.cs
namespace SIMS.Services
{
    public class AuthorizationService : IAuthorizationService
    {
        // Template Method: Định nghĩa skeleton của authorization flow
        public IActionResult EnsureAdmin(HttpContext context, Func<IActionResult> onSuccess)
        {
            var role = GetCurrentRole(context);
            
            // Step 1: Check authorization (template step)
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectBasedOnRole(role); // Hook method
            }

            // Step 2: Execute action if authorized (template step)
            return onSuccess();
        }

        public IActionResult EnsureStudent(HttpContext context, Func<IActionResult> onSuccess)
        {
            var role = GetCurrentRole(context);
            
            // Same template, different role check
            if (string.IsNullOrEmpty(role) || role != "Student")
            {
                return RedirectBasedOnRole(role);
            }

            return onSuccess();
        }

        public IActionResult EnsureFaculty(HttpContext context, Func<IActionResult> onSuccess)
        {
            var role = GetCurrentRole(context);
            
            // Same template, different role check
            if (string.IsNullOrEmpty(role) || role != "Faculty")
            {
                return RedirectBasedOnRole(role);
            }

            return onSuccess();
        }

        // Hook method: Có thể được override hoặc customize
        private RedirectToActionResult RedirectBasedOnRole(string? role)
        {
            return role switch
            {
                "Student" => new RedirectToActionResult("Index", "StudentDashboard", null),
                "Faculty" => new RedirectToActionResult("Index", "FacultyDashboard", null),
                "Admin" => new RedirectToActionResult("Index", "Dashboard", null),
                _ => new RedirectToActionResult("Index", "Login", null)
            };
        }

        // Helper methods (used in template)
        public string? GetCurrentRole(HttpContext context)
        {
            return context.Session.GetString("Role");
        }

        public int? GetCurrentUserId(HttpContext context)
        {
            var userIdString = context.Session.GetString("UserId");
            if (int.TryParse(userIdString, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
```

**Giải thích:**
- **Template Method**: `EnsureAdmin`, `EnsureStudent`, `EnsureFaculty` đều follow cùng một pattern:
  1. Get current role
  2. Check if authorized
  3. If not authorized → redirect
  4. If authorized → execute action
- **Hook Method**: `RedirectBasedOnRole` có thể được customize để redirect khác nhau
- **Reusability**: Code không bị duplicate, chỉ khác nhau ở role check

#### 5.2. Sử dụng trong Controller

```csharp
// SIMS/Controllers/StudentController.cs
namespace SIMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly IAuthorizationService _authorizationService;

        public StudentController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        public IActionResult Index(string searchString)
        {
            // Sử dụng template method
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                // Action chỉ được execute nếu user là Admin
                ViewData["Title"] = "Manage Students";
                var students = _studentService.GetAllStudents(searchString);
                return View(students.ToList());
            });
        }

        [HttpPost]
        public IActionResult Create(StudentViewModel model)
        {
            // Same template method, different action
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    var (success, errorMessage) = _studentService.CreateStudent(model);
                    // ... handle result
                }
                return View(model);
            });
        }
    }
}
```

**Giải thích:**
- Controller sử dụng template method để đảm bảo authorization
- Code trong lambda (`() => { ... }`) chỉ được execute nếu user có quyền
- Nếu không có quyền, template method tự động redirect

### Lợi ích của Template Method Pattern:

✅ **Code Reuse**: Tránh duplicate code giữa các methods tương tự

✅ **Consistency**: Đảm bảo tất cả authorization checks follow cùng một pattern

✅ **Maintainability**: Thay đổi authorization logic ở một nơi, áp dụng cho tất cả

✅ **Flexibility**: Có thể customize hook methods (`RedirectBasedOnRole`) mà không thay đổi template

---

## TỔNG KẾT VÀ SO SÁNH

### Bảng so sánh các Design Patterns:

| Pattern | Mục đích | Vị trí trong dự án | Lợi ích chính |
|---------|----------|-------------------|---------------|
| **Repository** | Tách biệt data access | `Repositories/` | Testability, Flexibility |
| **Service** | Encapsulate business logic | `Services/` | Separation of Concerns, Reusability |
| **Dependency Injection** | Loose coupling | `Program.cs`, Constructors | Testability, Maintainability |
| **Factory** | Tạo objects phức tạp | `CustomWebApplicationFactory` | Encapsulation, Testability |
| **Template Method** | Định nghĩa algorithm skeleton | `AuthorizationService` | Code Reuse, Consistency |

### Mối quan hệ giữa các Patterns:

```
Controller (uses DI)
    ↓ injects
Service (uses Repository Pattern)
    ↓ injects
Repository (uses DI)
    ↓ uses
DbContext (Entity Framework)

Factory Pattern → Creates test application với configuration đặc biệt
Template Method → Đảm bảo consistency trong authorization flow
```

### Kết luận:

Các design patterns trong dự án SIMS **bổ sung cho nhau** để tạo ra một architecture:
- ✅ **Maintainable**: Dễ đọc, dễ sửa
- ✅ **Testable**: Dễ viết unit/integration/E2E tests
- ✅ **Scalable**: Dễ thêm features mới
- ✅ **SOLID-compliant**: Tuân thủ các nguyên lý SOLID

Việc áp dụng các patterns này giúp codebase **professional**, **production-ready** và dễ dàng **onboard** developers mới.

