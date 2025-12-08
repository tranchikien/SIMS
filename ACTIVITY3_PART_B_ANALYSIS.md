# ACTIVITY 3 - PART B: PHÂN TÍCH VÀ ĐÁNH GIÁ CÁC PHƯƠNG PHÁP KIỂM THỬ TỰ ĐỘNG

## B. Analyze the Advantages and Disadvantages of Automated Testing Methods

### 1. UNIT TESTING

#### Example 1: The test runs very quickly and is easy to identify errors

**Illustrative Code:**

```csharp
// SIMS.Tests/AuthenticationServiceTests.cs
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
```

**Advantages:**

+ **Runs very fast**: Only uses fake repository (in-memory list), no database or HTTP required. Runtime: < 1ms/test. Can run hundreds of unit tests in seconds → suitable for running after each build.

+ **Easy to identify errors**: When the test fails, immediately know the error is in `AuthenticationService.Authenticate()`, no need to debug through many layers such as Controller → Service → Repository → DB.

**Limitations:**

+ **Cannot detect database configuration errors**: Because of using fake repository, cannot check for incorrect Entity Framework configuration, incorrect entity mapping → SQL table, or database constraints (unique index, foreign key).

**PHÂN TÍCH VÀ ĐÁNH GIÁ:**

**Phân tích:**
- Unit test tập trung vào **logic nghiệp vụ thuần túy** của một class/service, không phụ thuộc vào infrastructure (database, HTTP, file system).
- Sử dụng **Fake/Mock objects** để thay thế dependencies, cho phép test chạy độc lập và nhanh chóng.
- Phù hợp với **Test-Driven Development (TDD)**: viết test trước, sau đó implement code để pass test.

**Đánh giá:**
- **Khi nào nên dùng**: 
  - Test business logic phức tạp (validation, calculation, transformation)
  - Test edge cases và error handling
  - Test các method có nhiều conditional branches
  - Cần feedback nhanh trong quá trình development
  
- **Khi nào KHÔNG nên dùng**:
  - Test database schema và relationships
  - Test integration giữa các components
  - Test end-to-end user flows
  - Test performance và scalability

**Kết luận:** Unit testing là nền tảng của testing pyramid, chiếm 70-80% số lượng test cases. Nó đảm bảo từng unit code hoạt động đúng, nhưng không đảm bảo các units kết hợp với nhau hoạt động đúng.

---

#### Example 2: Protecting important business logic

**Illustrative Code:**

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
        FullName = "New Student",
        Email = "new@example.com",
        StudentId = "S001", // duplicate
        Program = "IT",
        Password = "pass123"
    };

    // Act
    var (success, error) = service.CreateStudent(model);

    // Assert
    Assert.False(success);
    Assert.Equal("Student ID already exists.", error);
}
```

**Advantages:**

+ **Protecting business rules**: This test ensures that the "StudentId must be unique" rule is always executed. If someone accidentally deletes the `IsStudentIdUnique` check, the test will fail immediately → protecting data integrity.

**Disadvantages:**

+ **Must write a lot of fake code**: For this test to run, the entire `IStudentRepository` must be implemented in the fake repository:

```csharp
private sealed class FakeStudentRepository : IStudentRepository
{
    public List<Student> Students { get; } = new();

    public IEnumerable<Student> GetAll() => Students;
    public Student? GetById(int id) => Students.FirstOrDefault(s => s.Id == id);
    public Student? GetByStudentId(string studentId) => 
        Students.FirstOrDefault(s => string.Equals(s.StudentId, studentId, StringComparison.OrdinalIgnoreCase));
    // ... many more methods
}
```

+ **Long test code, costly to maintain**: If repository interface changes (adds method), must edit corresponding fake.

**PHÂN TÍCH VÀ ĐÁNH GIÁ:**

**Phân tích:**
- Unit test này **bảo vệ business rule quan trọng**: tính duy nhất của StudentId. Đây là một **critical business rule** mà nếu vi phạm sẽ gây lỗi dữ liệu nghiêm trọng.
- Việc phải viết fake repository là **trade-off cần thiết** để đạt được tốc độ và tính độc lập của test.
- Fake repository phải **mimic behavior** của real repository một cách chính xác, đặc biệt là logic so sánh (case-sensitive/insensitive).

**Đánh giá:**
- **Lợi ích lớn hơn chi phí**: Mặc dù phải viết fake code, nhưng việc bảo vệ business rule quan trọng này là **critical**. Nếu không có test này, một developer có thể vô tình xóa validation và gây ra lỗi production.
- **Maintenance cost**: Khi interface thay đổi, phải update cả fake repository. Tuy nhiên, đây là **acceptable trade-off** vì:
  - Interface thường ổn định sau khi design hoàn thiện
  - Có thể dùng mocking framework (Moq, NSubstitute) để giảm code, nhưng fake repository cho phép control tốt hơn
- **Best practice**: Nên group các fake repositories vào một namespace riêng (`SIMS.Tests.Fakes`) để dễ tái sử dụng.

**Kết luận:** Unit test cho business rules là **essential**, đặc biệt là các rules liên quan đến data integrity. Chi phí viết fake code là **worth it** so với rủi ro mất dữ liệu hoặc lỗi logic.

---

#### Example 3: Can create "false positive" if fake repository has wrong logic

**Illustrative Code:**

```csharp
// SIMS.Tests/AuthenticationServiceTests.cs
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

    // Act - login bằng email thay vì username
    var (success, role, userInfo) = service.Authenticate("student8@example.com", "password");

    // Assert
    Assert.True(success);
    Assert.Equal("Student", role);
}
```

**Advantages:**

+ **Full control of test data**: Can simulate any situation (user does not exist, student lacks ReferenceId, user Inactive...) easily.

**Disadvantages:**

+ **Risk of "false positive"**: If fake repository has wrong logic (for example, `GetByUsernameOrEmail` is not case-sensitive like real implementation), test can pass but real code fails. This requires programmers to be careful when writing fake repository.

**PHÂN TÍCH VÀ ĐÁNH GIÁ:**

**Phân tích:**
- **False positive** là khi test PASS nhưng code thực tế FAIL. Đây là **rủi ro nghiêm trọng** vì tạo cảm giác an toàn giả.
- Nguyên nhân chính: **Fake repository không match behavior** của real repository:
  - Case sensitivity/insensitivity
  - Null handling
  - Exception throwing
  - Performance characteristics (async/sync)
  
**Đánh giá:**
- **Mức độ rủi ro**: 
  - **CAO** nếu fake repository được viết vội, không test kỹ
  - **THẤP** nếu fake repository được review và maintain cẩn thận
  
- **Giải pháp giảm thiểu**:
  1. **Code review**: Fake repository phải được review bởi senior developer
  2. **Integration test backup**: Có integration test để catch false positive
  3. **Contract testing**: Đảm bảo fake và real repository có cùng contract
  4. **Documentation**: Ghi rõ assumptions và limitations của fake repository

- **So sánh với Mocking Framework**:
  - **Fake repository (manual)**: 
    - ✅ Full control, dễ debug
    - ❌ Nhiều code, dễ sai logic
  - **Mocking framework (Moq)**:
    - ✅ Ít code, tự động generate
    - ❌ Khó setup complex scenarios, khó debug

**Kết luận:** False positive là **rủi ro thực tế** của unit testing với fake objects. Cần **cẩn thận** khi viết fake code và **backup bằng integration tests**. Tuy nhiên, lợi ích của unit testing (tốc độ, isolation) vẫn **vượt trội** so với rủi ro này nếu được implement đúng cách.

---

### 2. INTEGRATION TESTING

#### Example: Detecting data relationship errors between tables

**Illustrative Code:**

```csharp
// SIMS.Tests/StudentServiceIntegrationTests.cs
[Fact]
public void DeleteStudent_RemovesStudentUserAndEnrollments_FromDatabase()
{
    using var context = CreateInMemoryContext();

    // Seed data
    var student = new Student { StudentId = "S300", FullName = "Student 300", ... };
    context.Students.Add(student);
    context.SaveChanges();

    var user = new User { Username = "S300", ReferenceId = student.Id, ... };
    context.Users.Add(user);

    context.Enrollments.Add(new Enrollment { StudentId = student.Id, CourseId = 1 });
    context.SaveChanges();

    // Create real repositories and service
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
```

**Advantages:**

+ **Detecting data relationship errors**: This test uses a real repository + a real DbContext (InMemory), can check:
  - Is the Service deleting Student, User and Enrollment correctly?
  - Is the foreign key relationship working correctly?
  - Unit test cannot detect this error because it uses a fake repository.

**Disadvantages:**

+ **Setup is more complicated than unit test**: Must create DbContext, seed many tables (Students, Users, Enrollments). Test code is longer, harder to read than unit test.

+ **When the test fails, it is difficult to determine the cause**: The error can come from Service, Repository, or DbContext configuration. Must debug deep into EF Core to find the cause.

**PHÂN TÍCH VÀ ĐÁNH GIÁ:**

**Phân tích:**
- Integration test **kiểm tra sự tương tác** giữa các components (Service + Repository + DbContext), không chỉ logic đơn lẻ.
- Sử dụng **InMemory database** để tránh phụ thuộc vào SQL Server thực, nhưng vẫn test được EF Core configuration và relationships.
- Test này **phát hiện lỗi** mà unit test không thể: cascade delete, foreign key constraints, transaction handling.

**Đánh giá:**
- **Khi nào nên dùng**:
  - Test **data persistence**: CRUD operations có được lưu đúng vào DB không?
  - Test **relationships**: Foreign keys, cascade deletes có hoạt động không?
  - Test **transactions**: Rollback khi có lỗi có đúng không?
  - Test **EF Core configuration**: Entity mappings, indexes, constraints có đúng không?

- **Khi nào KHÔNG nên dùng**:
  - Test business logic đơn giản (đã có unit test)
  - Test UI/UX flows (nên dùng E2E)
  - Test performance (InMemory không phản ánh SQL Server thực)

- **So sánh với Unit Test**:
  | Aspect | Unit Test | Integration Test |
  |--------|-----------|------------------|
  | Speed | < 1ms | ~50-100ms |
  | Isolation | High | Medium |
  | Database errors | ❌ | ✅ |
  | Setup complexity | Low | Medium |
  | Debug difficulty | Easy | Hard |

- **Best practices**:
  1. **Isolate test data**: Mỗi test dùng database riêng (GUID-based name)
  2. **Cleanup**: Dispose DbContext sau mỗi test
  3. **Seed helper**: Tạo helper methods để seed data dễ dàng
  4. **Test one thing**: Mỗi test chỉ focus vào một integration scenario

**Kết luận:** Integration testing là **bước bổ sung quan trọng** cho unit testing. Nó phát hiện lỗi ở **tầng integration** mà unit test không thể. Tuy setup phức tạp hơn và chậm hơn, nhưng **worth it** để đảm bảo các components hoạt động đúng cùng nhau.

---

### 3. END-TO-END TESTING (E2E Testing)

#### Example: Testing the entire system from HTTP to database

**Illustrative Code:**

```csharp
// SIMS.Tests/LoginE2ETests.cs
[Fact]
public async Task Login_RedirectsToStudentDashboard_WhenValidStudentCredentials()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act - Submit login form
    var response = await client.PostAsync("/Login/Index", new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("Username", "S999"),
        new KeyValuePair<string, string>("Password", "password")
    }));

    // Assert
    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Contains("/StudentDashboard/Index", response.Headers.Location?.ToString());
}
```

**Advantages:**

+ **Testing the entire system**: This test checks both:
  - Routing (`/Login/Index`)
  - Controller processes the request
  - Service authenticates
  - Session is set
  - Redirect to the correct page

**Disadvantages:**

+ **Slowest to run**: Must start the application, create DbContext, seed data, process HTTP. Runtime: ~800ms/test (compared to <1ms of unit test). Cannot write too many E2E tests because it will slow down the build/test process.

+ **Difficult to debug when failed**: If the test fails, must look at the HTTP status code, response body (HTML), application log. Difficult to determine exactly where the error is (routing, controller, service, or DB).

**PHÂN TÍCH VÀ ĐÁNH GIÁ:**

**Phân tích:**
- E2E test **mô phỏng user thực** sử dụng hệ thống từ đầu đến cuối, không bỏ qua bất kỳ layer nào.
- Test này **đảm bảo** rằng toàn bộ flow hoạt động đúng: từ HTTP request → Controller → Service → Repository → Database → Response.
- Sử dụng **WebApplicationFactory** để tạo test server, cho phép test real HTTP requests/responses.

**Đánh giá:**
- **Khi nào nên dùng**:
  - Test **critical user flows**: Login, Registration, Payment, etc.
  - Test **routing và navigation**: URLs có đúng không? Redirects có đúng không?
  - Test **session và authentication**: Cookies, sessions có được set đúng không?
  - Test **integration với external services** (nếu có): APIs, email services, etc.

- **Khi nào KHÔNG nên dùng**:
  - Test business logic đơn giản (đã có unit test)
  - Test edge cases (nên dùng unit test)
  - Test performance (quá chậm)
  - Test UI styling (nên dùng manual testing hoặc visual regression testing)

- **So sánh với các phương pháp khác**:
  | Aspect | Unit Test | Integration Test | E2E Test |
  |--------|-----------|------------------|----------|
  | Speed | < 1ms | ~50-100ms | ~500-1000ms |
  | Coverage | Single component | Multiple components | Entire system |
  | Setup | Easy | Medium | Complex |
  | Debug | Easy | Medium | Hard |
  | Cost | Low | Medium | High |

- **Testing Pyramid**:
  ```
         /\
        /  \      E2E Tests (10%)
       /    \
      /______\    Integration Tests (20%)
     /        \
    /__________\  Unit Tests (70%)
  ```
  - **70% Unit Tests**: Nhanh, nhiều, test business logic
  - **20% Integration Tests**: Test component interactions
  - **10% E2E Tests**: Test critical user flows

- **Best practices**:
  1. **Minimize E2E tests**: Chỉ test critical paths, không test mọi scenario
  2. **Use page objects**: Tạo Page Object Model để dễ maintain
  3. **Parallel execution**: Chạy E2E tests song song để giảm thời gian
  4. **Test data management**: Dùng test database riêng, cleanup sau mỗi test
  5. **Failure screenshots**: Capture screenshots khi test fail để debug dễ hơn

**Kết luận:** E2E testing là **tầng cao nhất** của testing pyramid, đảm bảo hệ thống hoạt động đúng từ góc nhìn của user. Tuy chậm và khó debug, nhưng **essential** để đảm bảo quality ở production. Nên **giới hạn số lượng** E2E tests và chỉ focus vào **critical user journeys**.

---

## TỔNG KẾT VÀ KHUYẾN NGHỊ

### So sánh tổng quan:

| Tiêu chí | Unit Test | Integration Test | E2E Test |
|---------|-----------|------------------|----------|
| **Tốc độ** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐ |
| **Isolation** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐ |
| **Phát hiện lỗi DB** | ❌ | ✅ | ✅ |
| **Phát hiện lỗi routing** | ❌ | ❌ | ✅ |
| **Dễ debug** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ |
| **Chi phí maintain** | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Coverage** | Single method | Multiple components | Entire system |

### Khuyến nghị:

1. **Ưu tiên Unit Tests** (70%): Viết nhiều unit tests cho business logic, validation, calculations
2. **Bổ sung Integration Tests** (20%): Test data persistence, relationships, transactions
3. **Giới hạn E2E Tests** (10%): Chỉ test critical user flows (login, registration, payment)

### Kết luận cuối cùng:

Mỗi phương pháp testing có **vai trò riêng** và **bổ sung cho nhau**:
- **Unit Tests**: Đảm bảo từng component hoạt động đúng
- **Integration Tests**: Đảm bảo các components tương tác đúng
- **E2E Tests**: Đảm bảo hệ thống hoạt động đúng từ góc nhìn user

**Không có phương pháp nào là "tốt nhất"** - cần **kết hợp cả ba** để đạt được coverage và confidence cao nhất với chi phí hợp lý.

