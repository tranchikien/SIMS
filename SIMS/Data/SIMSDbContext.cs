using Microsoft.EntityFrameworkCore;
using SIMS.Models;

namespace SIMS.Data
{
    public class SIMSDbContext : DbContext
    {
        public SIMSDbContext(DbContextOptions<SIMSDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<AdminProfile> AdminProfiles { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(255);
                entity.Property(e => e.Gender).HasMaxLength(10);
                
                // Add check constraint for Role
                entity.ToTable(t => t.HasCheckConstraint("CK_User_Role", "Role IN ('Admin', 'Faculty', 'Student')"));
                // Add check constraint for Status
                entity.ToTable(t => t.HasCheckConstraint("CK_User_Status", "Status IN ('Active', 'Inactive')"));
                // Add check constraint for Gender
                entity.ToTable(t => t.HasCheckConstraint("CK_User_Gender", "Gender IS NULL OR Gender IN ('Male', 'Female', 'Other')"));
            });

            // Configure Student entity
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.StudentId).IsUnique();
                entity.Property(e => e.StudentId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Program).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                
                // Add check constraint for Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Student_Status", "Status IN ('Active', 'Inactive')"));
            });

            // Configure Faculty entity
            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.FacultyId).IsUnique();
                entity.Property(e => e.FacultyId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                
                // Add check constraint for Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Faculty_Status", "Status IN ('Active', 'Inactive')"));
            });

            // Configure Course entity
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.CourseCode).IsUnique();
                entity.Property(e => e.CourseCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CourseName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                
                // Add check constraint for Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Course_Status", "Status IN ('Active', 'Inactive')"));
            });

            // Configure Enrollment entity
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Enrolled");
                
                // Add check constraint for Status
                entity.ToTable(t => t.HasCheckConstraint("CK_Enrollment_Status", "Status IN ('Enrolled', 'Completed', 'Dropped')"));
                
                // Configure relationships
                entity.HasOne<Student>()
                    .WithMany()
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne<Course>()
                    .WithMany()
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne<Faculty>()
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Grade entity
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.EnrollmentId).IsUnique();
                entity.Property(e => e.LetterGrade).HasMaxLength(10);
                entity.Property(e => e.Comment).HasMaxLength(1000);
                
                // Configure relationships
                entity.HasOne<Enrollment>()
                    .WithMany()
                    .HasForeignKey(e => e.EnrollmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Configure Faculty relationship (optional)
                entity.HasOne<Faculty>()
                    .WithMany()
                    .HasForeignKey(e => e.FacultyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure AdminProfile entity
            modelBuilder.Entity<AdminProfile>(entity =>
            {
                entity.HasKey(e => e.Username);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(100);
            });

            // Configure ActivityLog entity
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PerformedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.OldValue).HasMaxLength(2000);
                entity.Property(e => e.NewValue).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired();
                
                // Indexes for better query performance
                entity.HasIndex(e => e.ActivityType);
                entity.HasIndex(e => e.StudentId);
                entity.HasIndex(e => e.CourseId);
                entity.HasIndex(e => e.FacultyId);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure Notification entity
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.NotificationType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RecipientRole).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsRead).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
        }
    }
}

