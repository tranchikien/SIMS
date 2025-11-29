using Microsoft.AspNetCore.Mvc;
using SIMS.Models;
using SIMS.Services;

namespace SIMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAuthenticationService _authenticationService;

        public LoginController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (success, role, userInfo) = _authenticationService.Authenticate(model.Username, model.Password);
                
                if (success)
                {
                    _authenticationService.SetSession(HttpContext, role, model.Username, userInfo);
                    
                    return role switch
                    {
                        "Admin" => RedirectToAction("Index", "Dashboard"),
                        "Student" => RedirectToAction("Index", "StudentDashboard"),
                        "Faculty" => RedirectToAction("Index", "FacultyDashboard"),
                        _ => RedirectToAction("Index", "Login")
                    };
                }

                // Invalid account
                ViewData["InvalidAccount"] = "Your Account Invalid";
                return View();
            }
            return View(model);
        }

        // Logout action
        public IActionResult Logout()
        {
            _authenticationService.ClearSession(HttpContext);
            return RedirectToAction("Index", "Login");
        }
    }
}
