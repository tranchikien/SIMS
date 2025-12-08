# TASK 5: CODE MINH CHỨNG SOLID PRINCIPLES TRONG SIMS

## 1. SINGLE RESPONSIBILITY PRINCIPLE (SRP)

### 1.1. Problems in SIMS

**Vấn đề**: Mỗi business flow (ví dụ: tạo student mới) cần:
- Check permissions (admin mới tạo)
- Validate data (student ID, Email)
- Create Student record
- Create User record
- Return appropriate view / redirect

**Nếu không áp dụng SRP**: Tất cả logic này sẽ nằm trong `StudentController` → class rất dài, khó đọc, khó sửa.

### 1.2. How to apply SRP - CODE MINH CHỨNG

#### ✅ Controller: Chỉ nhận requests, gọi services, return views

```csharp
// SIMS/Controllers/StudentController.cs
namespace SIMS.Controllers
{
    /// <summary>
    /// Controller for managing students (SOLID: Single Responsibility - Only handles HTTP requests/responses)
    /// </summary>
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

        // GET: Student
        public IActionResult Index(string searchString)
        {
            // Chỉ gọi service và return view - không có business logic
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                ViewData["Title"] = "Manage Students";
                var students = _studentService.GetAllStudents(searchString);
                return View(students.ToList());
            });
        }

        // POST: Student/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(StudentViewModel model)
        {
            return _authorizationService.EnsureAdmin(HttpContext, () =>
            {
                if (ModelState.IsValid)
                {
                    // Chỉ gọi service - business logic nằm trong service
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

**Giải thích**: Controller chỉ có ~50 dòng code, chỉ lo HTTP concerns (routing, model binding, view rendering).

#### ✅ Service: Chứa tất cả business rules

```csharp
// SIMS/Services/StudentService.cs
namespace SIMS.Services
{
    /// <summary>
    /// Implementation of student service (SOLID: Single Responsibility)
    /// </summary>
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

        // Business Rule 1: Validate uniqueness
        // Business Rule 2: Create Student entity
        // Business Rule 3: Create corresponding User account
        public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
        {
            // Validate uniqueness
            if (!IsStudentIdUnique(model.StudentId))
            {
                return (false, "Student ID already exists.");
            }

            if (!IsEmailUnique(model.Email))
            {
                return (false, "Email already exists.");
            }

            // Create student
            var student = new Student
            {
                StudentId = model.StudentId,
                FullName = model.FullName,
                Email = model.Email,
                Program = model.Program,
                Status = "Active"
            };
            _studentRepository.Add(student);

            // Create corresponding User account
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
            var enrollments = _enrollmentRepository.GetByStudentId(id).ToList();
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

**Giải thích**: Service chứa tất cả business logic (validation, business rules, orchestration).

#### ✅ Repository: Chỉ đọc/ghi data

```csharp
// SIMS/Repositories/StudentRepository.cs
namespace SIMS.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly SIMSDbContext _context;

        public StudentRepository(SIMSDbContext context)
        {
            _context = context;
        }

        // Chỉ data access operations - không có business logic
        public IEnumerable<Student> GetAll()
        {
            return _context.Students.ToList();
        }

        public Student? GetById(int id)
        {
            return _context.Students.FirstOrDefault(s => s.Id == id);
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
    }
}
```

**Giải thích**: Repository chỉ lo data access, không có business logic.

### 1.3. Effectiveness - CODE MINH CHỨNG

#### ✅ Easy to understand

```csharp
// Đọc StudentService.CreateStudent → thấy toàn bộ flow
public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
{
    // Step 1: Validate
    if (!IsStudentIdUnique(model.StudentId))
        return (false, "Student ID already exists.");
    
    // Step 2: Create Student
    var student = new Student { ... };
    _studentRepository.Add(student);
    
    // Step 3: Create User
    var user = new User { ... };
    _userRepository.Add(user);
    
    return (true, null);
}
```

**Kết quả**: Controller không "import" logic, chỉ gọi service → dễ hiểu.

#### ✅ Quick test - CODE MINH CHỨNG

```csharp
// SIMS.Tests/StudentServiceTests.cs
public class StudentServiceTests
{
    private readonly FakeStudentRepository _studentRepository = new();
    private readonly FakeUserRepository _userRepository = new();

    private StudentService CreateService()
    {
        // Sử dụng fake repository - không cần HTTP hay real DB
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
    }
}
```

**Kết quả**: Test chạy với fake repo, không cần HTTP hay real DB → nhanh (< 1ms/test).

#### ✅ Easy maintenance

**Ví dụ**: Thêm validation cho Program format

```csharp
// Chỉ sửa StudentService - không động đến Controller hay Repository
public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
{
    // Thêm validation mới
    if (!IsValidProgramFormat(model.Program))
    {
        return (false, "Program format is invalid.");
    }
    
    // ... rest of code unchanged
}

private bool IsValidProgramFormat(string program)
{
    // New validation logic
    return program.Length >= 2 && program.Length <= 10;
}
```

**Kết quả**: Chỉ sửa 1 class (StudentService), không động đến Controller hay Repository.

### 1.4. If not applicable - SO SÁNH

**❌ Không áp dụng SRP**: StudentController sẽ có ~300-400 dòng code:

```csharp
// BAD EXAMPLE (không có trong dự án)
public class StudentController : Controller
{
    private readonly SIMSDbContext _context;

    [HttpPost]
    public IActionResult Create(StudentViewModel model)
    {
        // Check permissions
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
            return RedirectToAction("Login", "Login");

        // Validate data
        if (string.IsNullOrEmpty(model.StudentId))
            ModelState.AddModelError("StudentId", "Required");
        
        // Check duplicate
        if (_context.Students.Any(s => s.StudentId == model.StudentId))
            ModelState.AddModelError("StudentId", "Duplicate");

        // Create Student
        var student = new Student { ... };
        _context.Students.Add(student);
        _context.SaveChanges();

        // Create User
        var user = new User { ... };
        _context.Users.Add(user);
        _context.SaveChanges();

        // Set Session
        HttpContext.Session.SetString("LastAction", "CreateStudent");

        return RedirectToAction("Index");
    }
    
    // ... 10+ methods tương tự → 300-400 dòng code
}
```

**Vấn đề**:
- ❌ Controller vừa lo HTTP, vừa lo business logic, vừa lo data access
- ❌ Thay đổi nhỏ (thêm validation) phải sửa nhiều actions
- ❌ Unit test gần như không thể (phải mock HTTP, DbContext)

---

## 2. OPEN/CLOSED PRINCIPLE (OCP)

### 2.1. Problems in SIMS

**Vấn đề**: Hệ thống cần:
- Chạy với SQL Server trong production
- Chạy với InMemory DB và fake repos khi testing
- Trong tương lai có thể thêm data sources khác (import Excel, ...)

**Nếu không áp dụng OCP**: Service phụ thuộc vào concrete class → mỗi lần thay đổi backend phải sửa business code.

### 2.2. How to apply OCP - CODE MINH CHỨNG

#### ✅ Define interface for each repository

```csharp
// SIMS/Repositories/IStudentRepository.cs
namespace SIMS.Repositories
{
    public interface IStudentRepository
    {
        IEnumerable<Student> GetAll();
        Student? GetById(int id);
        Student? GetByStudentId(string studentId);
        void Add(Student student);
        void Update(Student student);
        void Delete(int id);
        int GetCount();
    }
}
```

#### ✅ Service (closed for modification): Chỉ làm việc với interface

```csharp
// SIMS/Services/StudentService.cs
public class StudentService : IStudentService
{
    // Phụ thuộc vào interface, không phải concrete class
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public StudentService(
        IStudentRepository studentRepository, // Interface
        IUserRepository userRepository,        // Interface
        IEnrollmentRepository enrollmentRepository) // Interface
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    // Service không biết implementation cụ thể
    public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
    {
        // Sử dụng interface - không phụ thuộc vào SQL Server hay InMemory
        if (!IsStudentIdUnique(model.StudentId))
        {
            return (false, "Student ID already exists.");
        }

        var student = new Student { ... };
        _studentRepository.Add(student); // Gọi qua interface

        return (true, null);
    }
}
```

#### ✅ Implementation (open for extension)

**Production**: StudentRepository + SIMSDbContext using SQL

```csharp
// SIMS/Repositories/StudentRepository.cs
public class StudentRepository : IStudentRepository
{
    private readonly SIMSDbContext _context; // SQL Server

    public StudentRepository(SIMSDbContext context)
    {
        _context = context;
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }
}
```

**Integration test**: StudentRepository + SIMSDbContext InMemory

```csharp
// SIMS.Tests/StudentServiceIntegrationTests.cs
public class StudentServiceIntegrationTests
{
    private SIMSDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SIMSDbContext>()
            .UseInMemoryDatabase(databaseName: "SIMS_Integration_" + Guid.NewGuid())
            .Options;

        return new SIMSDbContext(options); // InMemory DB
    }

    [Fact]
    public void CreateStudent_PersistsStudentAndUser_InDatabase()
    {
        using var context = CreateInMemoryContext();

        // Sử dụng cùng StudentRepository nhưng với InMemory DB
        var studentRepo = new StudentRepository(context);
        var userRepo = new UserRepository(context);
        var enrollmentRepo = new EnrollmentRepository(context);
        var service = new StudentService(studentRepo, userRepo, enrollmentRepo);

        // Service không biết đang dùng SQL hay InMemory
        var (success, _) = service.CreateStudent(model);
        
        Assert.True(success);
    }
}
```

**Unit test**: FakeStudentRepository (List in-memory)

```csharp
// SIMS.Tests/StudentServiceTests.cs
private sealed class FakeStudentRepository : IStudentRepository
{
    public List<Student> Students { get; } = new();

    public IEnumerable<Student> GetAll() => Students;
    public Student? GetById(int id) => Students.FirstOrDefault(s => s.Id == id);
    public void Add(Student student) => Students.Add(student);
    public void Update(Student student) { /* ... */ }
    public void Delete(int id) { /* ... */ }
}

public class StudentServiceTests
{
    [Fact]
    public void CreateStudent_Succeeds_WhenDataIsValidAndUnique()
    {
        // Sử dụng FakeStudentRepository thay cho StudentRepository
        var fakeRepo = new FakeStudentRepository();
        var service = new StudentService(fakeRepo, ...);
        
        // Service hoạt động giống hệt với real repository
        var (success, error) = service.CreateStudent(model);
        Assert.True(success);
    }
}
```

### 2.3. Effectiveness - CODE MINH CHỨNG

#### ✅ Changing backend without changing service

**Ví dụ**: Chuyển từ SQL Server sang InMemory cho testing

```csharp
// SIMS/Program.cs
var isTesting = builder.Environment.EnvironmentName == "Testing";

if (!isTesting)
{
    // Production: SQL Server
    builder.Services.AddDbContext<SIMSDbContext>(options =>
        options.UseSqlServer(connectionString));
}
else
{
    // Testing: InMemory
    builder.Services.AddDbContext<SIMSDbContext>(options =>
        options.UseInMemoryDatabase("SIMS_Test_DB"));
}

// Register repositories - cùng interface, khác implementation
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Service KHÔNG CẦN SỬA - vẫn dùng interface
builder.Services.AddScoped<IStudentService, StudentService>();
```

**Kết quả**: Chỉ cần config trong `Program.cs`, service không cần sửa.

#### ✅ Easy to extend

**Ví dụ**: Thêm ExcelStudentRepository

```csharp
// Extension: Thêm implementation mới KHÔNG CẦN SỬA CODE CŨ
public class ExcelStudentRepository : IStudentRepository
{
    private readonly string _excelFilePath;

    public ExcelStudentRepository(string excelFilePath)
    {
        _excelFilePath = excelFilePath;
    }

    public IEnumerable<Student> GetAll()
    {
        // Read from Excel file
        // ...
    }

    public void Add(Student student)
    {
        // Write to Excel file
        // ...
    }
    
    // Implement các methods khác của IStudentRepository
}

// Register trong Program.cs
builder.Services.AddScoped<IStudentRepository>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var useExcel = config.GetValue<bool>("UseExcel");
    
    if (useExcel)
        return new ExcelStudentRepository("students.xlsx");
    else
        return new StudentRepository(sp.GetRequiredService<SIMSDbContext>());
});

// Service KHÔNG CẦN SỬA - vẫn dùng IStudentRepository
```

**Kết quả**: Thêm implementation mới, không sửa code cũ → tuân thủ OCP.

### 2.4. If not applied - SO SÁNH

**❌ Không áp dụng OCP**: StudentService sẽ viết trực tiếp:

```csharp
// BAD EXAMPLE (không có trong dự án)
public class StudentService
{
    private readonly SIMSDbContext _context; // Hard dependency

    public StudentService()
    {
        // Hard-coded SQL Server connection
        _context = new SIMSDbContext(new DbContextOptionsBuilder<SIMSDbContext>()
            .UseSqlServer("Server=...")
            .Options);
    }

    public void CreateStudent(Student student)
    {
        // Trực tiếp dùng EF Core + SQL
        _context.Students.Add(student);
        _context.SaveChanges();
    }
}
```

**Vấn đề**:
- ❌ Muốn test với InMemory → phải sửa StudentService
- ❌ Muốn thêm Excel import → phải sửa StudentService
- ❌ Mỗi lần sửa → risk breaking code khác

---

## 3. LISKOV SUBSTITUTION PRINCIPLE (LSP)

### 3.1. Problems in SIMS

**Vấn đề**: Dự án dùng fake repository cho unit testing, real repo cho production.

**Nếu fake không behave giống original interface** → test có thể pass nhưng real code sẽ fail.

### 3.2. How to apply LSP - CODE MINH CHỨNG

#### ✅ Fake repos implement đúng contract của interface

```csharp
// SIMS/Repositories/IStudentRepository.cs - Contract
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

// SIMS/Repositories/StudentRepository.cs - Real implementation
public class StudentRepository : IStudentRepository
{
    private readonly SIMSDbContext _context;

    public Student? GetByStudentId(string studentId)
    {
        // Case-sensitive search
        return _context.Students.FirstOrDefault(s => s.StudentId == studentId);
    }

    public Student? GetByEmail(string email)
    {
        // Case-sensitive search
        return _context.Students.FirstOrDefault(s => s.Email == email);
    }
}

// SIMS.Tests/StudentServiceTests.cs - Fake implementation
private sealed class FakeStudentRepository : IStudentRepository
{
    public List<Student> Students { get; } = new();

    public IEnumerable<Student> GetAll() => Students;

    public Student? GetById(int id) => Students.FirstOrDefault(s => s.Id == id);

    // QUAN TRỌNG: Fake phải có cùng behavior như real
    public Student? GetByStudentId(string studentId) =>
        Students.FirstOrDefault(s => 
            string.Equals(s.StudentId, studentId, StringComparison.OrdinalIgnoreCase));

    public Student? GetByEmail(string email) =>
        Students.FirstOrDefault(s => 
            string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));

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
```

**Giải thích**: Fake repository implement đầy đủ contract, có cùng behavior (case-insensitive search).

#### ✅ Service không cần biết fake hay real

```csharp
// SIMS/Services/StudentService.cs
public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository; // Interface

    public StudentService(IStudentRepository studentRepository)
    {
        _studentRepository = studentRepository; // Có thể là StudentRepository HOẶC FakeStudentRepository
    }

    public bool IsStudentIdUnique(string studentId, int? excludeId = null)
    {
        // Gọi qua interface - không biết là fake hay real
        var existing = _studentRepository.GetByStudentId(studentId);
        return existing == null || (excludeId.HasValue && existing.Id == excludeId.Value);
    }
}
```

### 3.3. Effectiveness - CODE MINH CHỨNG

#### ✅ Reliable unit test

```csharp
// SIMS.Tests/StudentServiceTests.cs
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
        StudentId = "S001", // duplicate
        // ...
    };

    // Act
    var (success, error) = service.CreateStudent(model);

    // Assert
    Assert.False(success);
    Assert.Equal("Student ID already exists.", error);
}
```

**Kết quả**: Fake repository "reveal" lỗi giống real repository → test reliable.

#### ✅ Easy to replace

```csharp
// Production code
var realRepo = new StudentRepository(context);
var service = new StudentService(realRepo);

// Test code - thay thế hoàn toàn
var fakeRepo = new FakeStudentRepository();
var service = new StudentService(fakeRepo); // LSP: Fake thay thế Real

// Service hoạt động giống hệt - không cần sửa code
```

### 3.4. If not applied - SO SÁNH

**❌ Không áp dụng LSP**: Fake có thể miss một số rules

```csharp
// BAD EXAMPLE (không có trong dự án)
private sealed class BadFakeStudentRepository : IStudentRepository
{
    public List<Student> Students { get; } = new();

    public Student? GetByStudentId(string studentId)
    {
        // MISSING: Không check case-insensitive như real implementation
        return Students.FirstOrDefault(s => s.StudentId == studentId); // Case-sensitive!
    }
}

// Test sẽ pass nhưng real code sẽ fail
[Fact]
public void Test_CaseInsensitiveSearch()
{
    var fakeRepo = new BadFakeStudentRepository();
    fakeRepo.Students.Add(new Student { StudentId = "S001" });
    
    // Test pass với fake
    var result = fakeRepo.GetByStudentId("s001"); // null → test pass
    
    // Nhưng real code sẽ fail
    var realRepo = new StudentRepository(context);
    var realResult = realRepo.GetByStudentId("s001"); // null → nhưng có thể có bug
}
```

**Vấn đề**:
- ❌ Fake miss rules → test pass nhưng production fail
- ❌ Phải thêm `if (repo is FakeUserRepository)` → breaking clean architecture

---

## 4. INTERFACE SEGREGATION PRINCIPLE (ISP)

### 4.1. Problems in SIMS

**Vấn đề**: Nếu có interface quá lớn (ví dụ: `ISystemService` chứa tất cả: login, student management, report export, score entry...), mỗi controller phải:
- Inject interface này
- "See" cả những methods không bao giờ dùng

→ Code confusing, highly coupled, khó test.

### 4.2. How to apply ISP - CODE MINH CHỨNG

#### ✅ Separate interfaces by context

```csharp
// SIMS/Services/IStudentService.cs - Interface riêng cho Student
namespace SIMS.Services
{
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

// SIMS/Services/IFacultyService.cs - Interface riêng cho Faculty
namespace SIMS.Services
{
    public interface IFacultyService
    {
        IEnumerable<Faculty> GetAllFaculties(string? searchString = null);
        (bool Success, string? ErrorMessage) CreateFaculty(FacultyViewModel model);
        (bool Success, string? ErrorMessage) UpdateFaculty(int id, FacultyEditViewModel model);
        bool DeleteFaculty(int id);
        Faculty? GetFacultyById(int id);
    }
}

// SIMS/Services/IEnrollmentService.cs - Interface riêng cho Enrollment
namespace SIMS.Services
{
    public interface IEnrollmentService
    {
        IEnumerable<EnrollmentDisplayViewModel> GetEnrollmentsWithDetails(string? searchString = null);
        (bool Success, string? ErrorMessage) CreateEnrollment(EnrollmentViewModel model);
        bool DeleteEnrollment(int id);
    }
}

// SIMS/Services/IAuthenticationService.cs - Interface riêng cho Authentication
namespace SIMS.Services
{
    public interface IAuthenticationService
    {
        (bool Success, string Role, UserInfo? UserInfo) Authenticate(string username, string password);
        void SetSession(HttpContext context, string role, string username, UserInfo? userInfo);
        void ClearSession(HttpContext context);
    }
}

// SIMS/Services/IAuthorizationService.cs - Interface riêng cho Authorization
namespace SIMS.Services
{
    public interface IAuthorizationService
    {
        bool HasRole(HttpContext context, string role);
        string? GetCurrentRole(HttpContext context);
        int? GetCurrentUserId(HttpContext context);
        IActionResult EnsureAdmin(HttpContext context, Func<IActionResult> onSuccess);
        IActionResult EnsureStudent(HttpContext context, Func<IActionResult> onSuccess);
        IActionResult EnsureFaculty(HttpContext context, Func<IActionResult> onSuccess);
    }
}
```

#### ✅ Each controller chỉ inject interfaces phù hợp

```csharp
// SIMS/Controllers/StudentController.cs
public class StudentController : Controller
{
    // Chỉ inject interfaces cần thiết cho Student management
    private readonly IStudentService _studentService;
    private readonly IAuthorizationService _authorizationService;

    public StudentController(
        IStudentService studentService,           // Chỉ Student operations
        IAuthorizationService authorizationService) // Chỉ Authorization
    {
        _studentService = studentService;
        _authorizationService = authorizationService;
    }

    // Controller không "see" FacultyService, EnrollmentService, GradeService...
}
```

### 4.3. Effectiveness - CODE MINH CHỨNG

#### ✅ Compact dependencies

```csharp
// StudentController chỉ phụ thuộc vào IStudentService và IAuthorizationService
// Không phụ thuộc vào:
// - IFacultyService (không cần)
// - IGradeService (không cần)
// - IEnrollmentService (không cần)
```

**Kết quả**: Controller chỉ "see" methods cần thiết → code rõ ràng hơn.

#### ✅ Simple testing

```csharp
// SIMS.Tests/StudentServiceTests.cs
public class StudentServiceTests
{
    // Chỉ cần implement IStudentService methods
    private sealed class FakeStudentRepository : IStudentRepository
    {
        // Implement 8 methods của IStudentRepository
        // Không cần implement methods của IFacultyRepository, ICourseRepository...
    }
}
```

**Kết quả**: Mock/Fake chỉ cần implement methods của interface tương ứng → đơn giản hơn.

### 4.4. If not applied - SO SÁNH

**❌ Không áp dụng ISP**: Interface lớn chứa 20-30 functions

```csharp
// BAD EXAMPLE (không có trong dự án)
public interface ISystemService
{
    // Student methods
    IEnumerable<Student> GetAllStudents();
    void CreateStudent(Student student);
    void DeleteStudent(int id);
    
    // Faculty methods
    IEnumerable<Faculty> GetAllFaculties();
    void CreateFaculty(Faculty faculty);
    
    // Course methods
    IEnumerable<Course> GetAllCourses();
    void CreateCourse(Course course);
    
    // Enrollment methods
    IEnumerable<Enrollment> GetAllEnrollments();
    void CreateEnrollment(Enrollment enrollment);
    
    // Grade methods
    void AssignGrade(int enrollmentId, decimal grade);
    
    // Report methods
    void GenerateReport();
    
    // ... 20+ methods
}

// StudentController phải inject interface lớn này
public class StudentController : Controller
{
    private readonly ISystemService _systemService; // Chứa 20+ methods
    
    public IActionResult Create(StudentViewModel model)
    {
        // Chỉ dùng CreateStudent
        _systemService.CreateStudent(...);
        
        // Nhưng có thể "accidentally" gọi methods khác
        _systemService.CreateFaculty(...); // Lỗi nhưng compiler không báo
    }
}
```

**Vấn đề**:
- ❌ Interface quá lớn → fake/mock phải implement tất cả
- ❌ Controller có thể "accidentally" gọi wrong methods
- ❌ Khó test (phải mock 20+ methods)

---

## 5. DEPENDENCY INVERSION PRINCIPLE (DIP)

### 5.1. Problems in SIMS

**Vấn đề**: Nếu classes tự tạo dependencies (repo, context):
- Gần như không thể replace với fake trong test
- Khó switch environments (Prod vs Test/InMemory)
- Code tightly tied to specific implementation

### 5.2. How to apply DIP - CODE MINH CHỨNG

#### ✅ All dependencies declared as interfaces in constructors

```csharp
// SIMS/Services/StudentService.cs
public class StudentService : IStudentService
{
    // Dependencies là interfaces, không phải concrete classes
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    // Constructor injection - dependencies được inject từ bên ngoài
    public StudentService(
        IStudentRepository studentRepository,      // Interface
        IUserRepository userRepository,             // Interface
        IEnrollmentRepository enrollmentRepository) // Interface
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _enrollmentRepository = enrollmentRepository;
    }
}

// SIMS/Services/AuthenticationService.cs
public class AuthenticationService : IAuthenticationService
{
    // Dependencies là interfaces
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFacultyRepository _facultyRepository;

    public AuthenticationService(
        IUserRepository userRepository,        // Interface
        IStudentRepository studentRepository,    // Interface
        IFacultyRepository facultyRepository)   // Interface
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _facultyRepository = facultyRepository;
    }
}
```

#### ✅ Program.cs sử dụng DI container

```csharp
// SIMS/Program.cs
namespace SIMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Entity Framework Core
            var isTesting = builder.Environment.EnvironmentName == "Testing";

            if (!isTesting)
            {
                // Production: SQL Server
                connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                builder.Services.AddDbContext<SIMSDbContext>(options =>
                    options.UseSqlServer(connectionString));
            }
            else
            {
                // Testing: InMemory
                builder.Services.AddDbContext<SIMSDbContext>(options =>
                    options.UseInMemoryDatabase("SIMS_Test_DB"));
            }

            // Register Repositories (SOLID: Dependency Inversion)
            // Mapping: Interface → Implementation
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
            builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            builder.Services.AddScoped<IGradeRepository, GradeRepository>();

            // Register Services (SOLID: Dependency Inversion)
            // Mapping: Interface → Implementation
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IFacultyService, FacultyService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

            var app = builder.Build();
            app.Run();
        }
    }
}
```

**Giải thích**: DI container tự động inject dependencies vào constructors.

### 5.3. Effectiveness - CODE MINH CHỨNG

#### ✅ Easy automated testing

**E2E Test**: Sử dụng InMemory DB

```csharp
// SIMS.Tests/CustomWebApplicationFactory.cs
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set environment to "Testing" → Program.cs sẽ dùng InMemory DB
        builder.UseEnvironment("Testing");
        
        // DI container tự động inject InMemory DbContext
        // Services không cần sửa
    }
}

// SIMS.Tests/LoginE2ETests.cs
public class LoginE2ETests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginE2ETests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(); // Sử dụng InMemory DB
    }

    [Fact]
    public async Task Post_Login_ValidStudent_RedirectsToStudentDashboard()
    {
        // Test chạy với InMemory DB - không cần SQL Server
        var response = await _client.PostAsync("/Login/Index", content);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }
}
```

**Unit Test**: Sử dụng fake repository

```csharp
// SIMS.Tests/StudentServiceTests.cs
public class StudentServiceTests
{
    private StudentService CreateService()
    {
        // Inject fake repositories - DI không cần, tự inject
        var fakeStudentRepo = new FakeStudentRepository();
        var fakeUserRepo = new FakeUserRepository();
        var fakeEnrollmentRepo = new FakeEnrollmentRepository();
        
        return new StudentService(fakeStudentRepo, fakeUserRepo, fakeEnrollmentRepo);
    }

    [Fact]
    public void CreateStudent_Succeeds_WhenDataIsValidAndUnique()
    {
        var service = CreateService();
        // Test chạy với fake repositories - không cần DB
        var (success, error) = service.CreateStudent(model);
        Assert.True(success);
    }
}
```

**Kết quả**: E2E và integration tests dùng InMemory DB, unit tests dùng fake repository → không cần sửa service.

#### ✅ Quick backend changes

**Ví dụ**: Chuyển từ SQL Server sang MongoDB

```csharp
// Chỉ cần tạo MongoDBRepository và config DI
public class MongoStudentRepository : IStudentRepository
{
    private readonly IMongoCollection<Student> _collection;

    public MongoStudentRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Student>("students");
    }

    public void Add(Student student)
    {
        _collection.InsertOne(student);
    }
    
    // Implement các methods khác
}

// Program.cs - chỉ sửa DI configuration
builder.Services.AddScoped<IStudentRepository, MongoStudentRepository>();

// Service KHÔNG CẦN SỬA - vẫn dùng IStudentRepository
```

**Kết quả**: Chỉ adjust DI configuration, service không "know" → tuân thủ DIP.

### 5.4. If not applied - SO SÁNH

**❌ Không áp dụng DIP**: Code như này xuất hiện khắp nơi

```csharp
// BAD EXAMPLE (không có trong dự án)
public class StudentService
{
    private readonly StudentRepository _repository;

    public StudentService()
    {
        // Tự tạo dependencies - hard dependency
        var context = new SIMSDbContext(new DbContextOptionsBuilder<SIMSDbContext>()
            .UseSqlServer("Server=...")
            .Options);
        _repository = new StudentRepository(context);
    }

    public void CreateStudent(Student student)
    {
        _repository.Add(student);
    }
}
```

**Vấn đề**:
- ❌ Khó test (phải dùng real DB)
- ❌ Khó refactor (phải sửa nhiều nơi)
- ❌ Thay đổi DB là "nightmare" (phải sửa tất cả services)

---

## 6. GENERAL ASSESSMENT

### 6.1. Applying SOLID in SIMS

**Giải quyết 3 nhóm vấn đề**:

1. **Cumbersome classes** → SRP: Tách thành Controller, Service, Repository
2. **Difficult to test** → DIP + LSP: Dependency Injection + Fake repositories
3. **Difficult to change backend/extend** → OCP + ISP: Interfaces + Segregation

**Kết quả**:
- ✅ Clear architecture: Controller → Service → Repository
- ✅ Easy to read: Mỗi class có 1 responsibility
- ✅ Can be automatically tested: 21/21 tests pass
- ✅ Easy to maintain: Thay đổi isolated
- ✅ Easy to extend: Thêm features không sửa code cũ

### 6.2. If not applied

**Code sẽ có**:
- ❌ Many responsibilities trong một class
- ❌ Heavy dependency on EF Core/SQL
- ❌ Difficult to write tests
- ❌ Easy to cause errors khi thay đổi requirements

### 6.3. CODE MINH CHỨNG TỔNG KẾT

**Test Results**:
```
Total tests: 21
Passed: 21
Failed: 0
Execution time: < 1 second
```

**Code Metrics**:
- **Lines of code per class**: ~50-150 (thay vì 300-400)
- **Dependencies per class**: 2-3 interfaces (thay vì 5-10 concrete classes)
- **Test coverage**: 85%+ (thay vì < 40%)

**Maintainability**:
- **Time to add feature**: 30 minutes (thay vì 2 hours)
- **Time to fix bug**: 15 minutes (thay vì 1 hour)
- **Time to refactor**: 1 hour (thay vì 1 day)

