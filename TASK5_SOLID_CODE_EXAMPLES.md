# TASK 5: CODE MINH HỌA SOLID PRINCIPLES

## 1. SINGLE RESPONSIBILITY PRINCIPLE (SRP)

### 1.1. Problems in SIMS

Mỗi business flow (ví dụ: tạo student mới) cần:
- Check permissions (admin mới tạo)
- Validate data (student ID, Email)
- Create Student record
- Create User record
- Return appropriate view / redirect

Nếu không áp dụng SRP: Tất cả logic này sẽ nằm trong `StudentController` → class rất dài, khó đọc, khó sửa.

### 1.2. How to apply SRP - CODE MINH HỌA

#### ❌ KHÔNG ÁP DỤNG SRP (Bad Example):

```csharp
// BAD: Class làm quá nhiều việc
public class StudentController : Controller
{
    private readonly SIMSDbContext _context;

    [HttpPost]
    public IActionResult Create(StudentViewModel model)
    {
        // Responsibility 1: Check permissions
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return RedirectToAction("Login", "Login");
        }

        // Responsibility 2: Validate data
        if (string.IsNullOrEmpty(model.StudentId))
        {
            ModelState.AddModelError("StudentId", "Student ID is required");
        }
        if (string.IsNullOrEmpty(model.Email) || !model.Email.Contains("@"))
        {
            ModelState.AddModelError("Email", "Invalid email format");
        }

        // Responsibility 3: Check duplicate
        if (_context.Students.Any(s => s.StudentId == model.StudentId))
        {
            ModelState.AddModelError("StudentId", "Student ID already exists");
        }

        // Responsibility 4: Create Student
        var student = new Student
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            Program = model.Program,
            Status = "Active"
        };
        _context.Students.Add(student);
        _context.SaveChanges();

        // Responsibility 5: Create User
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
        _context.Users.Add(user);
        _context.SaveChanges();

        // Responsibility 6: Set Session
        HttpContext.Session.SetString("LastAction", "CreateStudent");

        // Responsibility 7: Log activity
        var log = $"Student {model.StudentId} created at {DateTime.Now}";
        File.AppendAllText("activity.log", log + Environment.NewLine);

        return RedirectToAction("Index");
    }
}
```

**Vấn đề**: Class có 7 responsibilities → 7 lý do để thay đổi → vi phạm SRP.

#### ✅ ÁP DỤNG SRP (Good Example):

```csharp
// Responsibility 1: HTTP Requests/Responses ONLY
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
    public IActionResult Create(StudentViewModel model)
    {
        // Chỉ lo HTTP: check authorization, call service, return view
        return _authorizationService.EnsureAdmin(HttpContext, () =>
        {
            if (ModelState.IsValid)
            {
                var (success, errorMessage) = _studentService.CreateStudent(model);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Student created successfully!";
                    return RedirectToAction("Index");
                }
                
                ModelState.AddModelError(string.Empty, errorMessage);
            }
            
            return View(model);
        });
    }
}

// Responsibility 2: Business Logic ONLY
public class StudentService : IStudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;

    public StudentService(
        IStudentRepository studentRepository,
        IUserRepository userRepository)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
    }

    public (bool Success, string? ErrorMessage) CreateStudent(StudentViewModel model)
    {
        // Business Rule 1: Validate
        if (string.IsNullOrEmpty(model.StudentId))
        {
            return (false, "Student ID is required");
        }

        // Business Rule 2: Check duplicate
        if (_studentRepository.GetByStudentId(model.StudentId) != null)
        {
            return (false, "Student ID already exists");
        }

        // Business Rule 3: Create Student
        var student = new Student
        {
            StudentId = model.StudentId,
            FullName = model.FullName,
            Email = model.Email,
            Program = model.Program,
            Status = "Active"
        };
        _studentRepository.Add(student);

        // Business Rule 4: Create User
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
}

// Responsibility 3: Data Access ONLY
public class StudentRepository : IStudentRepository
{
    private readonly SIMSDbContext _context;

    public StudentRepository(SIMSDbContext context)
    {
        _context = context;
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }

    public Student? GetByStudentId(string studentId)
    {
        return _context.Students.FirstOrDefault(s => s.StudentId == studentId);
    }
}
```

**Lợi ích**:
- ✅ Mỗi class chỉ có 1 responsibility
- ✅ Dễ test (mock repository cho service)
- ✅ Dễ maintain (thay đổi business logic không ảnh hưởng controller)

### 1.3. Effectiveness

**Easy to understand**: Đọc `StudentService.CreateStudent` → thấy toàn bộ business flow; controller không "import" logic.

**Quick test**: 
```csharp
// Test service với fake repository - không cần HTTP hay DB
var fakeRepo = new FakeStudentRepository();
var service = new StudentService(fakeRepo, fakeUserRepo);
var (success, error) = service.CreateStudent(model);
Assert.True(success);
```

**Easy maintenance**: Thay đổi rules (ví dụ: thêm validation Program format) → chỉ sửa `StudentService`, không động đến controller hay repository.

### 1.4. If not applicable

**StudentController** sẽ có 300-400 dòng code, vừa lo ModelState, vừa query EF Core, vừa set Session → khó review, debug lâu.

Thay đổi nhỏ (thêm rules) phải sửa nhiều actions → dễ miss, dễ lỗi.

Unit test gần như không thể vì phải simulate request, reply, DbContext trong mỗi test.

---

## 2. OPEN/CLOSED PRINCIPLE (OCP)

### 2.1. Problems in SIMS

Hệ thống cần:
- Chạy với SQL Server trong production
- Chạy với InMemory DB và fake repos khi testing
- Trong tương lai có thể thêm data sources khác (import Excel, ...)

Nếu không áp dụng OCP: Service phụ thuộc vào concrete class → mỗi lần thay đổi backend phải sửa business code.

### 2.2. How to apply OCP - CODE MINH HỌA

#### ❌ KHÔNG ÁP DỤNG OCP (Bad Example):

```csharp
// BAD: Service phụ thuộc trực tiếp vào SQL Server
public class StudentService
{
    private readonly SIMSDbContext _context;

    public StudentService()
    {
        // Hard-coded SQL Server connection
        var options = new DbContextOptionsBuilder<SIMSDbContext>()
            .UseSqlServer("Server=localhost;Database=SIMS;...")
            .Options;
        _context = new SIMSDbContext(options);
    }

    public void CreateStudent(Student student)
    {
        // Trực tiếp dùng EF Core + SQL Server
        _context.Students.Add(student);
        _context.SaveChanges();
    }

    public List<Student> GetAllStudents()
    {
        return _context.Students.ToList();
    }
}

// Muốn test với InMemory → phải sửa StudentService
// Muốn thêm Excel import → phải sửa StudentService
// → Vi phạm OCP (open for modification)
```

#### ✅ ÁP DỤNG OCP (Good Example):

```csharp
// Step 1: Define Interface (Abstraction)
public interface IStudentRepository
{
    void Add(Student student);
    Student? GetById(int id);
    Student? GetByStudentId(string studentId);
    List<Student> GetAll();
}

// Step 2: Service depends on Interface (Closed for modification)
public class StudentService
{
    private readonly IStudentRepository _repository;

    // Service không biết implementation cụ thể
    public StudentService(IStudentRepository repository)
    {
        _repository = repository; // Interface, không phải concrete class
    }

    public void CreateStudent(Student student)
    {
        // Sử dụng interface - không phụ thuộc SQL Server hay InMemory
        _repository.Add(student);
    }

    public List<Student> GetAllStudents()
    {
        return _repository.GetAll();
    }
}

// Step 3: Implementations (Open for extension)

// Implementation 1: SQL Server (Production)
public class StudentRepository : IStudentRepository
{
    private readonly SIMSDbContext _context;

    public StudentRepository(SIMSDbContext context)
    {
        _context = context; // SQL Server
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }

    public List<Student> GetAll()
    {
        return _context.Students.ToList();
    }
}

// Implementation 2: InMemory (Testing)
public class InMemoryStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new();

    public void Add(Student student)
    {
        _students.Add(student);
    }

    public List<Student> GetAll()
    {
        return _students;
    }
}

// Implementation 3: Excel (Future extension - không cần sửa code cũ)
public class ExcelStudentRepository : IStudentRepository
{
    private readonly string _excelFilePath;

    public ExcelStudentRepository(string excelFilePath)
    {
        _excelFilePath = excelFilePath;
    }

    public void Add(Student student)
    {
        // Write to Excel file
        // ...
    }

    public List<Student> GetAll()
    {
        // Read from Excel file
        // ...
    }
}

// Step 4: Dependency Injection Configuration
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var isTesting = builder.Environment.EnvironmentName == "Testing";

        if (!isTesting)
        {
            // Production: SQL Server
            builder.Services.AddDbContext<SIMSDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        }
        else
        {
            // Testing: InMemory
            builder.Services.AddDbContext<SIMSDbContext>(options =>
                options.UseInMemoryDatabase("TestDB"));
            builder.Services.AddScoped<IStudentRepository, InMemoryStudentRepository>();
        }

        // Service KHÔNG CẦN SỬA - vẫn dùng IStudentRepository
        builder.Services.AddScoped<StudentService>();

        var app = builder.Build();
        app.Run();
    }
}
```

**Lợi ích**:
- ✅ Thay đổi backend → chỉ config DI, không sửa service
- ✅ Thêm implementation mới → chỉ thêm class mới, không sửa code cũ
- ✅ Tuân thủ OCP (open for extension, closed for modification)

### 2.3. Effectiveness

**Changing backend without changing service**: Để chạy test, chỉ cần config `UseInMemoryDatabase` trong `Program.cs`, service không cần sửa.

**Easy to extend**: Muốn thêm `ExcelStudentRepository` → chỉ tạo class mới implement `IStudentRepository`, DI sẽ inject implementation phù hợp; logic cũ vẫn an toàn.

**Reduced risk**: Ít cần "touch" code cũ → ít regression bugs.

### 2.4. If not applied

**StudentService** sẽ viết trực tiếp `_context.Students.Add(...)` hoặc `new StudentRepository()` → tất cả đều hard-dependent vào EF Core + SQL.

Muốn test với InMemory hoặc fake repo → phải sửa mỗi service; mỗi lần sửa risk breaking các threads khác.

---

## 3. LISKOV SUBSTITUTION PRINCIPLE (LSP)

### 3.1. Problems in SIMS

Dự án dùng fake repository cho unit testing, real repo cho production.

Nếu fake không behave giống original interface → test có thể pass nhưng real code sẽ fail.

### 3.2. How to apply LSP - CODE MINH HỌA

#### ❌ KHÔNG ÁP DỤNG LSP (Bad Example):

```csharp
// BAD: Fake repository không implement đúng contract
public interface IStudentRepository
{
    Student? GetByStudentId(string studentId);
    void Add(Student student);
}

// Real implementation
public class StudentRepository : IStudentRepository
{
    private readonly SIMSDbContext _context;

    public Student? GetByStudentId(string studentId)
    {
        // Case-insensitive search
        return _context.Students.FirstOrDefault(s => 
            s.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }
}

// Fake implementation - VI PHẠM LSP
public class FakeStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new();

    public Student? GetByStudentId(string studentId)
    {
        // MISSING: Case-sensitive search (khác với real implementation)
        return _students.FirstOrDefault(s => s.StudentId == studentId); // Case-sensitive!
    }

    public void Add(Student student)
    {
        _students.Add(student);
        // MISSING: Không check duplicate như real implementation
    }
}

// Usage - Test sẽ pass nhưng production fail
public class StudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository;
    }

    public bool IsStudentIdUnique(string studentId)
    {
        // Test với fake: "S001" != "s001" → return true (PASS)
        // Production với real: "S001" == "s001" → return false (FAIL)
        return _repository.GetByStudentId(studentId) == null;
    }
}
```

**Vấn đề**: Fake không thể thay thế real → test unreliable.

#### ✅ ÁP DỤNG LSP (Good Example):

```csharp
// Interface định nghĩa contract rõ ràng
public interface IStudentRepository
{
    Student? GetByStudentId(string studentId);
    Student? GetByEmail(string email);
    void Add(Student student);
    void Update(Student student);
    void Delete(int id);
    List<Student> GetAll();
}

// Real implementation
public class StudentRepository : IStudentRepository
{
    private readonly SIMSDbContext _context;

    public Student? GetByStudentId(string studentId)
    {
        // Case-insensitive search
        return _context.Students.FirstOrDefault(s => 
            s.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    }

    public Student? GetByEmail(string email)
    {
        // Case-insensitive search
        return _context.Students.FirstOrDefault(s => 
            s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }
}

// Fake implementation - TUÂN THỦ LSP
public class FakeStudentRepository : IStudentRepository
{
    private readonly List<Student> _students = new();

    // QUAN TRỌNG: Cùng behavior như real implementation
    public Student? GetByStudentId(string studentId)
    {
        // Case-insensitive search (giống real)
        return _students.FirstOrDefault(s => 
            s.StudentId.Equals(studentId, StringComparison.OrdinalIgnoreCase));
    }

    public Student? GetByEmail(string email)
    {
        // Case-insensitive search (giống real)
        return _students.FirstOrDefault(s => 
            s.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(Student student)
    {
        // Giống real: chỉ add, không check duplicate (business logic ở service)
        _students.Add(student);
    }

    public void Update(Student student)
    {
        var existing = _students.FirstOrDefault(s => s.Id == student.Id);
        if (existing != null)
        {
            _students.Remove(existing);
            _students.Add(student);
        }
    }

    public void Delete(int id)
    {
        var student = _students.FirstOrDefault(s => s.Id == id);
        if (student != null)
        {
            _students.Remove(student);
        }
    }

    public List<Student> GetAll()
    {
        return _students.ToList();
    }
}

// Service không cần biết fake hay real
public class StudentService
{
    private readonly IStudentRepository _repository;

    public StudentService(IStudentRepository repository)
    {
        _repository = repository; // Có thể là StudentRepository HOẶC FakeStudentRepository
    }

    public bool IsStudentIdUnique(string studentId)
    {
        // Hoạt động giống hệt với cả fake và real
        return _repository.GetByStudentId(studentId) == null;
    }
}

// Test - Fake thay thế Real hoàn toàn
[Fact]
public void IsStudentIdUnique_ReturnsTrue_WhenStudentIdNotExists()
{
    // Arrange
    var fakeRepo = new FakeStudentRepository();
    var service = new StudentService(fakeRepo); // LSP: Fake thay thế Real

    // Act
    var result = service.IsStudentIdUnique("S001");

    // Assert
    Assert.True(result); // Test reliable vì fake có cùng behavior như real
}
```

**Lợi ích**:
- ✅ Fake có thể thay thế real hoàn toàn
- ✅ Test reliable (fake có cùng behavior như real)
- ✅ Service không cần biết fake hay real

### 3.3. Effectiveness

**Reliable unit test**: Nếu logic sai, fake repo cũng "reveal" lỗi → test sẽ báo lỗi đúng chỗ.

**Easy to replace**: Trong test, có thể thay `StudentRepository` bằng `FakeStudentRepository` mà không cần sửa một dòng code trong service.

### 3.4. If not applied

**Fake** có thể miss một số rules (ví dụ: không check `Status == "Active"`) → test luôn pass dù real code sẽ fail trong production.

Dev phải thêm `if (repo is FakeUserRepository)` để handle khác nhau → breaking clean architecture.

---

## 4. INTERFACE SEGREGATION PRINCIPLE (ISP)

### 4.1. Problems in SIMS

Nếu có interface quá lớn (ví dụ: `ISystemService` chứa tất cả: login, student management, report export, score entry...), mỗi controller phải:
- Inject interface này
- "See" cả những methods không bao giờ dùng

→ Code confusing, highly coupled, khó test.

### 4.2. How to apply ISP - CODE MINH HỌA

#### ❌ KHÔNG ÁP DỤNG ISP (Bad Example):

```csharp
// BAD: Interface quá lớn (Fat Interface)
public interface ISystemService
{
    // Student methods
    void CreateStudent(Student student);
    void UpdateStudent(int id, Student student);
    void DeleteStudent(int id);
    List<Student> GetAllStudents();

    // Faculty methods
    void CreateFaculty(Faculty faculty);
    void UpdateFaculty(int id, Faculty faculty);
    void DeleteFaculty(int id);
    List<Faculty> GetAllFaculties();

    // Course methods
    void CreateCourse(Course course);
    void UpdateCourse(int id, Course course);
    void DeleteCourse(int id);
    List<Course> GetAllCourses();

    // Enrollment methods
    void CreateEnrollment(Enrollment enrollment);
    void DeleteEnrollment(int id);
    List<Enrollment> GetAllEnrollments();

    // Grade methods
    void AssignGrade(int enrollmentId, decimal grade);
    List<Grade> GetGradesByStudent(int studentId);

    // Report methods
    void GenerateStudentReport(int studentId);
    void GenerateFacultyReport(int facultyId);
    byte[] ExportToExcel();

    // Authentication methods
    bool Login(string username, string password);
    void Logout();
    
    // ... 30+ methods
}

// StudentController phải inject interface lớn này
public class StudentController : Controller
{
    private readonly ISystemService _systemService; // Chứa 30+ methods

    public StudentController(ISystemService systemService)
    {
        _systemService = systemService;
    }

    [HttpPost]
    public IActionResult Create(StudentViewModel model)
    {
        // Chỉ dùng CreateStudent
        _systemService.CreateStudent(new Student { ... });

        // Nhưng có thể "accidentally" gọi methods khác
        _systemService.CreateFaculty(...); // Lỗi nhưng compiler không báo
        _systemService.AssignGrade(...);    // Không cần nhưng vẫn "see"
    }
}

// Test - phải mock 30+ methods
public class StudentControllerTests
{
    [Fact]
    public void Create_CallsService()
    {
        var mockService = new Mock<ISystemService>();
        // Phải setup 30+ methods dù chỉ dùng 1 method
        mockService.Setup(s => s.CreateStudent(It.IsAny<Student>()));
        mockService.Setup(s => s.CreateFaculty(It.IsAny<Faculty>())); // Không cần
        mockService.Setup(s => s.AssignGrade(It.IsAny<int>(), It.IsAny<decimal>())); // Không cần
        // ... setup 27 methods nữa

        var controller = new StudentController(mockService.Object);
        // ...
    }
}
```

**Vấn đề**: Interface quá lớn → client phụ thuộc vào methods không cần → vi phạm ISP.

#### ✅ ÁP DỤNG ISP (Good Example):

```csharp
// Interface 1: Student operations ONLY
public interface IStudentService
{
    void CreateStudent(Student student);
    void UpdateStudent(int id, Student student);
    void DeleteStudent(int id);
    List<Student> GetAllStudents();
    Student? GetStudentById(int id);
}

// Interface 2: Faculty operations ONLY
public interface IFacultyService
{
    void CreateFaculty(Faculty faculty);
    void UpdateFaculty(int id, Faculty faculty);
    void DeleteFaculty(int id);
    List<Faculty> GetAllFaculties();
    Faculty? GetFacultyById(int id);
}

// Interface 3: Enrollment operations ONLY
public interface IEnrollmentService
{
    void CreateEnrollment(Enrollment enrollment);
    void DeleteEnrollment(int id);
    List<Enrollment> GetAllEnrollments();
}

// Interface 4: Grade operations ONLY
public interface IGradeService
{
    void AssignGrade(int enrollmentId, decimal grade);
    List<Grade> GetGradesByStudent(int studentId);
}

// Interface 5: Authentication operations ONLY
public interface IAuthenticationService
{
    bool Login(string username, string password);
    void Logout();
    UserInfo? GetCurrentUser();
}

// Interface 6: Authorization operations ONLY
public interface IAuthorizationService
{
    bool HasRole(string role);
    bool IsAdmin();
    bool IsStudent();
}

// StudentController chỉ inject interfaces cần thiết
public class StudentController : Controller
{
    private readonly IStudentService _studentService;        // Chỉ Student operations
    private readonly IAuthorizationService _authorizationService; // Chỉ Authorization

    public StudentController(
        IStudentService studentService,
        IAuthorizationService authorizationService)
    {
        _studentService = studentService;
        _authorizationService = authorizationService;
    }

    [HttpPost]
    public IActionResult Create(StudentViewModel model)
    {
        // Chỉ "see" methods từ IStudentService và IAuthorizationService
        if (!_authorizationService.IsAdmin())
        {
            return RedirectToAction("Login");
        }

        _studentService.CreateStudent(new Student { ... });
        return RedirectToAction("Index");
    }
}

// Test - chỉ mock interfaces cần thiết
public class StudentControllerTests
{
    [Fact]
    public void Create_CallsService()
    {
        var mockStudentService = new Mock<IStudentService>();
        mockStudentService.Setup(s => s.CreateStudent(It.IsAny<Student>()));

        var mockAuthService = new Mock<IAuthorizationService>();
        mockAuthService.Setup(a => a.IsAdmin()).Returns(true);

        var controller = new StudentController(
            mockStudentService.Object,
            mockAuthService.Object
        );
        // ...
    }
}
```

**Lợi ích**:
- ✅ Interface nhỏ, focused (segregated)
- ✅ Client chỉ phụ thuộc vào methods cần thiết
- ✅ Dễ test (chỉ mock interfaces cần thiết)
- ✅ Khó "accidentally" gọi wrong methods

### 4.3. Effectiveness

**Compact dependencies**: `StudentController` không phụ thuộc vào grading function hay faculty function.

**Few errors**: Khó "accidentally" gọi wrong role function vì interface không expose.

**Simple testing**: Khi mocking, chỉ implement vài methods trong interface tương ứng, không cần tạo fake lớn.

### 4.4. If not applied

**Interface** chứa 20-30 functions → fake/mock phải implement tất cả, confusing testing.

**Controllers** có thể mistakenly gọi admin function (do cùng interface) → ảnh hưởng security.

---

## 5. DEPENDENCY INVERSION PRINCIPLE (DIP)

### 5.1. Problems in SIMS

Nếu classes tự tạo dependencies (repo, context):
- Gần như không thể replace với fake trong test
- Khó switch environments (Prod vs Test/InMemory)
- Code tightly tied to specific implementation

### 5.2. How to apply DIP - CODE MINH HỌA

#### ❌ KHÔNG ÁP DỤNG DIP (Bad Example):

```csharp
// BAD: Classes tự tạo dependencies
public class StudentService
{
    private readonly StudentRepository _repository;
    private readonly SIMSDbContext _context;

    public StudentService()
    {
        // Tự tạo dependencies - hard dependency
        var options = new DbContextOptionsBuilder<SIMSDbContext>()
            .UseSqlServer("Server=localhost;Database=SIMS;...")
            .Options;
        _context = new SIMSDbContext(options);
        _repository = new StudentRepository(_context);
    }

    public void CreateStudent(Student student)
    {
        _repository.Add(student);
    }
}

// Vấn đề:
// - Không thể test (phải dùng real DB)
// - Không thể thay đổi implementation
// - Tight coupling
```

#### ✅ ÁP DỤNG DIP (Good Example):

```csharp
// Step 1: Define Abstractions (Interfaces)
public interface IStudentRepository
{
    void Add(Student student);
    Student? GetById(int id);
    List<Student> GetAll();
}

public interface IUserRepository
{
    void Add(User user);
    User? GetById(int id);
}

// Step 2: High-level module depends on Abstraction
public class StudentService
{
    // Phụ thuộc vào abstraction (interface), không phải concrete class
    private readonly IStudentRepository _studentRepository;
    private readonly IUserRepository _userRepository;

    // Constructor injection - dependencies được inject từ bên ngoài
    public StudentService(
        IStudentRepository studentRepository, // Abstraction
        IUserRepository userRepository)      // Abstraction
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
    }

    public void CreateStudent(Student student)
    {
        // Sử dụng abstraction, không biết implementation cụ thể
        _studentRepository.Add(student);
    }
}

// Step 3: Low-level modules implement Abstractions
public class StudentRepository : IStudentRepository
{
    // Low-level cũng phụ thuộc vào abstraction (DbContext)
    private readonly SIMSDbContext _context;

    public StudentRepository(SIMSDbContext context) // DbContext là abstraction
    {
        _context = context;
    }

    public void Add(Student student)
    {
        _context.Students.Add(student);
        _context.SaveChanges();
    }
}

// Step 4: Dependency Injection Container
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var isTesting = builder.Environment.EnvironmentName == "Testing";

        // Configure DbContext (Abstraction)
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
                options.UseInMemoryDatabase("TestDB"));
        }

        // Register Repositories: Interface → Implementation
        builder.Services.AddScoped<IStudentRepository, StudentRepository>();
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Register Services: Interface → Implementation
        builder.Services.AddScoped<IStudentService, StudentService>();

        var app = builder.Build();
        app.Run();
    }
}

// Step 5: Usage - DI Container tự động inject dependencies
public class StudentController : Controller
{
    private readonly IStudentService _studentService;

    // DI Container tự động inject IStudentService → StudentService
    // StudentService được inject IStudentRepository → StudentRepository
    // StudentRepository được inject SIMSDbContext → SQL Server hoặc InMemory
    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }
}
```

**Lợi ích**:
- ✅ High-level phụ thuộc vào abstraction (interface)
- ✅ Low-level cũng phụ thuộc vào abstraction
- ✅ Có thể thay đổi implementation (SQL → MongoDB → InMemory)
- ✅ Dễ test (dùng fake repository)

### 5.3. Effectiveness

**Easy automated testing**: 
- E2E và integration tests có thể dùng InMemory DB mà không cần sửa service
- Unit tests dùng fake repository qua DI

**Quick backend changes**: Muốn đổi từ SQL sang provider khác → chỉ adjust DI configuration; service không "know".

### 5.4. If not applied

**Code** như `var repo = new StudentRepository(new SIMSDbContext(...))` xuất hiện khắp nơi:
- Khó test
- Khó refactor
- Thay đổi DB là "nightmare"

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

