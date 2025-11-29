using SIMS.Models;
using Microsoft.AspNetCore.Http;

namespace SIMS.Services
{
    public interface IAuthenticationService
    {
        (bool Success, string Role, UserInfo? UserInfo) Authenticate(string username, string password);
        void SetSession(HttpContext context, string role, string username, UserInfo? userInfo);
        void ClearSession(HttpContext context);
    }

    public class UserInfo
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
    }
}

