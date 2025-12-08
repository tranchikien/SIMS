# Factory Method Pattern - Code Minh Họa Ngắn Gọn

## 1.5 Code minh họa thực tế trong SIMS (C# - Ngắn gọn, trực quan)

### Bước 1: Định nghĩa Interface và Concrete Classes

```csharp
// Interface chung cho tất cả loại người dùng
public interface IUser
{
    string Role { get; }
    void DisplayInfo();
}

// Concrete classes
public class Student : IUser
{
    public string StudentId { get; set; }
    public string Role => "Student";
    
    public void DisplayInfo() => Console.WriteLine($"Sinh viên - ID: {StudentId}");
}

public class Faculty : IUser
{
    public string Department { get; set; }
    public string Role => "Faculty";
    
    public void DisplayInfo() => Console.WriteLine($"Giảng viên - Khoa: {Department}");
}

public class Admin : IUser
{
    public string Role => "Admin";
    public void DisplayInfo() => Console.WriteLine("Quản trị viên");
}

public class Parent : IUser
{
    public string Role => "Parent";
    public void DisplayInfo() => Console.WriteLine("Phụ huynh");
}
```

### Bước 2: Factory Abstract Class và Concrete Factories

```csharp
// Factory trừu tượng
public abstract class UserFactory
{
    public abstract IUser CreateUser(string param);
    
    // Template method - logic chung cho đăng ký
    public void RegisterUser(string name, string param)
    {
        var user = CreateUser(param);
        Console.WriteLine($"Đăng ký thành công: {name}");
        user.DisplayInfo();
    }
}

// Concrete Factories
public class StudentFactory : UserFactory
{
    public override IUser CreateUser(string param) 
        => new Student { StudentId = param };
}

public class FacultyFactory : UserFactory
{
    public override IUser CreateUser(string param) 
        => new Faculty { Department = param };
}

public class AdminFactory : UserFactory
{
    public override IUser CreateUser(string param) 
        => new Admin();
}

public class ParentFactory : UserFactory
{
    public override IUser CreateUser(string param) 
        => new Parent();
}
```

### Bước 3: Factory Provider (Chọn Factory động)

```csharp
using System.Collections.Generic;

public static class FactoryProvider
{
    private static readonly Dictionary<string, UserFactory> _factories = new()
    {
        { "student", new StudentFactory() },
        { "faculty", new FacultyFactory() },
        { "admin", new AdminFactory() },
        { "parent", new ParentFactory() }
    };
    
    public static UserFactory GetFactory(string role)
    {
        if (!_factories.TryGetValue(role.ToLower(), out var factory))
            throw new ArgumentException($"Vai trò không hỗ trợ: {role}");
        
        return factory;
    }
}
```

### Bước 4: Sử dụng trong Service

```csharp
public class RegistrationService
{
    public void Register(string role, string name, string param)
    {
        var factory = FactoryProvider.GetFactory(role);
        factory.RegisterUser(name, param);
    }
}

// Sử dụng
class Program
{
    static void Main()
    {
        var service = new RegistrationService();
        
        service.Register("student", "Nguyen Van A", "SV001");
        service.Register("parent", "Le Thi B", null);
        
        // Thêm vai trò mới: chỉ cần thêm vào FactoryProvider
        // Không cần sửa RegistrationService!
    }
}
```

## Giải thích ngắn gọn:

1. **IUser**: Interface chung - đảm bảo tất cả User có cùng contract
2. **Concrete Classes**: Student, Faculty, Admin, Parent - mỗi loại có logic riêng
3. **UserFactory**: Abstract class định nghĩa cách tạo User (Factory Method)
4. **Concrete Factories**: Mỗi factory tạo một loại User cụ thể
5. **FactoryProvider**: Map chọn factory động dựa trên role
6. **RegistrationService**: Sử dụng factory mà không biết chi tiết tạo User như thế nào

## Lợi ích:

✅ **Mở rộng dễ dàng**: Thêm Staff role → chỉ cần tạo `StaffFactory` và thêm vào `FactoryProvider`

✅ **Không sửa code cũ**: `RegistrationService` không cần thay đổi khi thêm role mới

✅ **Tuân thủ Open/Closed**: Mở cho extension, đóng cho modification

✅ **Giảm coupling**: Service không phụ thuộc vào concrete User classes

