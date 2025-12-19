using SIMS.Models;
using SIMS.Repositories;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace SIMS.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IFacultyRepository _facultyRepository;
        private readonly IPasswordService _passwordService;

        public AuthenticationService(
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            IFacultyRepository facultyRepository,
            IPasswordService passwordService)
        {
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _facultyRepository = facultyRepository;
            _passwordService = passwordService;
        }

        public (bool Success, string Role, UserInfo? UserInfo) Authenticate(string username, string password)
        {
            // Check user in Users table
            var user = _userRepository.GetByUsernameOrEmail(username);
            
            // Verify password using PasswordService (supports both hashed and plain text for backward compatibility)
            bool isPasswordValid = false;
            if (user != null && user.Status == "Active")
            {
                // Try to verify as hashed password first
                isPasswordValid = _passwordService.VerifyPassword(password, user.Password);
                
                // If verification fails, check if it's a plain text password (for backward compatibility during migration)
                if (!isPasswordValid && user.Password == password)
                {
                    isPasswordValid = true;
                }
            }
            
            if (user != null && isPasswordValid && user.Status == "Active")
            {
                // Return user info based on role
                if (user.Role == "Student" && user.ReferenceId.HasValue)
                {
                    try
                    {
                        var student = _studentRepository.GetById(user.ReferenceId.Value);
                        if (student != null)
                        {
                            return (true, "Student", new UserInfo { Id = user.ReferenceId.Value, FullName = student.FullName });
                        }
                    }
                    catch
                    {
                        // Student not found with this ReferenceId
                        return (false, string.Empty, null);
                    }
                }
                else if (user.Role == "Faculty" && user.ReferenceId.HasValue)
                {
                    try
                    {
                        var faculty = _facultyRepository.GetById(user.ReferenceId.Value);
                        if (faculty != null)
                        {
                            return (true, "Faculty", new UserInfo { Id = user.ReferenceId.Value, FullName = faculty.FullName });
                        }
                    }
                    catch
                    {
                        // Faculty not found with this ReferenceId
                        return (false, string.Empty, null);
                    }
                }
                else if (user.Role == "Admin")
                {
                    return (true, "Admin", new UserInfo { Id = user.Id, FullName = user.FullName });
                }
            }

            return (false, string.Empty, null);
        }

        public void SetSession(HttpContext context, string role, string username, UserInfo? userInfo)
        {
            context.Session.SetString("Role", role);
            context.Session.SetString("Username", username);
            
            if (userInfo != null)
            {
                context.Session.SetString("UserId", userInfo.Id.ToString());
                context.Session.SetString("FullName", userInfo.FullName);
            }
        }

        public void ClearSession(HttpContext context)
        {
            context.Session.Clear();
        }
    }
}

