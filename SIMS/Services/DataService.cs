using System.Text.Json;
using SIMS.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SIMS.Services
{
    public class DataService
    {
        private static readonly string DataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Data");
        private static readonly string StudentsFile = Path.Combine(DataDirectory, "students.json");
        private static readonly string FacultiesFile = Path.Combine(DataDirectory, "faculties.json");
        private static readonly string CoursesFile = Path.Combine(DataDirectory, "courses.json");
        private static readonly string EnrollmentsFile = Path.Combine(DataDirectory, "enrollments.json");
        private static readonly string GradesFile = Path.Combine(DataDirectory, "grades.json");
        private static readonly string NextIdFile = Path.Combine(DataDirectory, "nextids.json");
        private static readonly string AdminProfileFile = Path.Combine(DataDirectory, "adminprofile.json");

        static DataService()
        {
            // Ensure data directory exists
            if (!Directory.Exists(DataDirectory))
            {
                Directory.CreateDirectory(DataDirectory);
            }
        }

        // Students
        public static List<Student> LoadStudents()
        {
            if (!File.Exists(StudentsFile))
            {
                // Return default data if file doesn't exist
                return new List<Student>
                {
                    new Student { Id = 1, FullName = "Nguyen Van A", Email = "nguyenvana@example.com", StudentId = "SE001", Program = "Software Engineering", Status = "Active" },
                    new Student { Id = 2, FullName = "Tran Thi B", Email = "tranthib@example.com", StudentId = "SE002", Program = "Computer Science", Status = "Active" },
                    new Student { Id = 3, FullName = "Le Van C", Email = "levanc@example.com", StudentId = "SE003", Program = "Information Technology", Status = "Inactive" }
                };
            }

            try
            {
                var json = File.ReadAllText(StudentsFile);
                return JsonSerializer.Deserialize<List<Student>>(json) ?? new List<Student>();
            }
            catch
            {
                return new List<Student>();
            }
        }

        public static void SaveStudents(List<Student> students)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(students, options);
            File.WriteAllText(StudentsFile, json);
        }

        // Faculties
        public static List<Faculty> LoadFaculties()
        {
            if (!File.Exists(FacultiesFile))
            {
                // Return default data if file doesn't exist
                return new List<Faculty>
                {
                    new Faculty { Id = 1, FullName = "Dr. Nguyen Van X", Email = "nguyenvanx@example.com", FacultyId = "FAC001", Department = "Computer Science", Status = "Active" },
                    new Faculty { Id = 2, FullName = "Prof. Tran Thi Y", Email = "tranthiy@example.com", FacultyId = "FAC002", Department = "Software Engineering", Status = "Active" },
                    new Faculty { Id = 3, FullName = "Dr. Le Van Z", Email = "levanz@example.com", FacultyId = "FAC003", Department = "Information Technology", Status = "Inactive" }
                };
            }

            try
            {
                var json = File.ReadAllText(FacultiesFile);
                return JsonSerializer.Deserialize<List<Faculty>>(json) ?? new List<Faculty>();
            }
            catch
            {
                return new List<Faculty>();
            }
        }

        public static void SaveFaculties(List<Faculty> faculties)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(faculties, options);
            File.WriteAllText(FacultiesFile, json);
        }

        // Courses
        public static List<Course> LoadCourses()
        {
            if (!File.Exists(CoursesFile))
            {
                // Return default data if file doesn't exist
                return new List<Course>
                {
                    new Course { Id = 1, CourseCode = "CS101", CourseName = "Introduction to Computer Science", Description = "Basic concepts of computer science", Credits = 3, Status = "Active" },
                    new Course { Id = 2, CourseCode = "SE201", CourseName = "Software Engineering", Description = "Principles and practices of software development", Credits = 4, Status = "Active" },
                    new Course { Id = 3, CourseCode = "IT301", CourseName = "Database Systems", Description = "Database design and management", Credits = 3, Status = "Active" },
                    new Course { Id = 4, CourseCode = "CS401", CourseName = "Web Development", Description = "Modern web development technologies", Credits = 4, Status = "Inactive" }
                };
            }

            try
            {
                var json = File.ReadAllText(CoursesFile);
                return JsonSerializer.Deserialize<List<Course>>(json) ?? new List<Course>();
            }
            catch
            {
                return new List<Course>();
            }
        }

        public static void SaveCourses(List<Course> courses)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(courses, options);
            File.WriteAllText(CoursesFile, json);
        }

        // Next IDs
        public static Dictionary<string, int> LoadNextIds()
        {
            if (!File.Exists(NextIdFile))
            {
                return new Dictionary<string, int>
                {
                    { "Student", 4 },
                    { "Faculty", 4 },
                    { "Course", 5 },
                    { "Enrollment", 5 },
                    { "Grade", 4 }
                };
            }

            try
            {
                var json = File.ReadAllText(NextIdFile);
                return JsonSerializer.Deserialize<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
            }
            catch
            {
                return new Dictionary<string, int>
                {
                    { "Student", 4 },
                    { "Faculty", 4 },
                    { "Course", 5 },
                    { "Enrollment", 5 },
                    { "Grade", 4 }
                };
            }
        }

        public static void SaveNextIds(Dictionary<string, int> nextIds)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(nextIds, options);
            File.WriteAllText(NextIdFile, json);
        }

        public static int GetNextId(string entityType)
        {
            var nextIds = LoadNextIds();
            if (!nextIds.ContainsKey(entityType))
            {
                nextIds[entityType] = 1;
            }
            var nextId = nextIds[entityType];
            nextIds[entityType] = nextId + 1;
            SaveNextIds(nextIds);
            return nextId;
        }

        // Enrollments
        public static List<Enrollment> LoadEnrollments()
        {
            if (!File.Exists(EnrollmentsFile))
            {
                // Return default sample enrollments
                return new List<Enrollment>
                {
                    new Enrollment { Id = 1, StudentId = 1, CourseId = 1, Status = "Enrolled", FacultyId = 1 },
                    new Enrollment { Id = 2, StudentId = 1, CourseId = 2, Status = "Enrolled", FacultyId = 2 },
                    new Enrollment { Id = 3, StudentId = 2, CourseId = 1, Status = "Enrolled", FacultyId = 1 },
                    new Enrollment { Id = 4, StudentId = 2, CourseId = 3, Status = "Enrolled", FacultyId = 3 }
                };
            }

            try
            {
                var json = File.ReadAllText(EnrollmentsFile);
                return JsonSerializer.Deserialize<List<Enrollment>>(json) ?? new List<Enrollment>();
            }
            catch
            {
                return new List<Enrollment>();
            }
        }

        public static void SaveEnrollments(List<Enrollment> enrollments)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(enrollments, options);
            File.WriteAllText(EnrollmentsFile, json);
        }

        // Grades
        public static List<Grade> LoadGrades()
        {
            if (!File.Exists(GradesFile))
            {
                // Return default sample grades (chỉ dùng FinalScore & TotalScore)
                return new List<Grade>
                {
                    new Grade { Id = 1, EnrollmentId = 1, StudentId = 1, CourseId = 1, FinalScore = 90, TotalScore = 90m, LetterGrade = "A" },
                    new Grade { Id = 2, EnrollmentId = 2, StudentId = 1, CourseId = 2, FinalScore = 82, TotalScore = 82m, LetterGrade = "B" },
                    new Grade { Id = 3, EnrollmentId = 3, StudentId = 2, CourseId = 1, FinalScore = 95, TotalScore = 95m, LetterGrade = "A" }
                };
            }

            try
            {
                var json = File.ReadAllText(GradesFile);
                return JsonSerializer.Deserialize<List<Grade>>(json) ?? new List<Grade>();
            }
            catch
            {
                return new List<Grade>();
            }
        }

        public static void SaveGrades(List<Grade> grades)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(grades, options);
            File.WriteAllText(GradesFile, json);
        }

        // Admin Profile
        public static AdminProfile LoadAdminProfile()
        {
            if (!File.Exists(AdminProfileFile))
            {
                return new AdminProfile
                {
                    FullName = "Dr. Nguyen Van X",
                    Email = "admin@sims.edu",
                    Username = "admin",
                    Role = "System Administrator"
                };
            }

            try
            {
                var json = File.ReadAllText(AdminProfileFile);
                return JsonSerializer.Deserialize<AdminProfile>(json) ?? new AdminProfile();
            }
            catch
            {
                return new AdminProfile
                {
                    FullName = "Dr. Nguyen Van X",
                    Email = "admin@sims.edu",
                    Username = "admin",
                    Role = "System Administrator"
                };
            }
        }

        public static void SaveAdminProfile(AdminProfile profile)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(profile, options);
            File.WriteAllText(AdminProfileFile, json);
        }
    }
}

