using Microsoft.EntityFrameworkCore;
using SIMS.Data;
using SIMS.Repositories;
using SIMS.Services;

namespace SIMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Configure Entity Framework Core with SQL Server
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            
            builder.Services.AddDbContext<SIMSDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            // Register Repositories (SOLID: Dependency Inversion)
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IStudentRepository, StudentRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();
            builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
            builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
            builder.Services.AddScoped<IGradeRepository, GradeRepository>();
            builder.Services.AddScoped<IAdminProfileRepository, AdminProfileRepository>();
            builder.Services.AddScoped<IIdGenerator, IdGenerator>();

            // Register Services (SOLID: Single Responsibility)
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
            builder.Services.AddScoped<IStudentDashboardService, StudentDashboardService>();
            builder.Services.AddScoped<IGradeService, GradeService>();
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IFacultyService, FacultyService>();
            builder.Services.AddScoped<ICourseService, CourseService>();
            builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

            var app = builder.Build();

            // Ensure database is created and migrated
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                
                try
                {
                    var context = services.GetRequiredService<SIMSDbContext>();
                    
                    logger.LogInformation("Đang kiểm tra kết nối database...");
                    
                    // Test connection first
                    if (!context.Database.CanConnect())
                    {
                        logger.LogWarning("Không thể kết nối đến database. Đang thử tạo database...");
                        context.Database.EnsureCreated();
                        logger.LogInformation("Database đã được tạo thành công!");
                    }
                    else
                    {
                        logger.LogInformation("Đã kết nối thành công đến database.");
                        
                        // Ensure tables exist
                        context.Database.EnsureCreated();
                    }
                }
                catch (Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    logger.LogError(sqlEx, "Lỗi SQL Server: {Message}", sqlEx.Message);
                    Console.WriteLine($"\n=== LỖI KẾT NỐI SQL SERVER ===");
                    Console.WriteLine($"Lỗi: {sqlEx.Message}");
                    Console.WriteLine($"Connection String: {connectionString}");
                    Console.WriteLine($"\nCÁCH KHẮC PHỤC:");
                    Console.WriteLine($"1. Kiểm tra SQL Server đã được cài đặt và đang chạy");
                    Console.WriteLine($"2. Kiểm tra tên server trong connection string:");
                    Console.WriteLine($"   - Nếu dùng SQL Server Express: Server=LAPTOP-743QFSIA\\SQLEXPRESS");
                    Console.WriteLine($"   - Nếu dùng SQL Server Default: Server=LAPTOP-743QFSIA");
                    Console.WriteLine($"   - Hoặc dùng LocalDB: Server=(localdb)\\mssqllocaldb");
                    Console.WriteLine($"3. Kiểm tra SQL Server Browser service đang chạy");
                    Console.WriteLine($"4. Kiểm tra firewall cho phép kết nối\n");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Lỗi không xác định: {Message}", ex.Message);
                    Console.WriteLine($"\n=== LỖI ===");
                    Console.WriteLine($"Message: {ex.Message}");
                    Console.WriteLine($"Connection String: {connectionString}\n");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
