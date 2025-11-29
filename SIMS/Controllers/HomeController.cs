using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SIMS.Models;

namespace SIMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public string Hello()
        {
            return "Hello word";
        }
        // https://localhost:port/Home/Hello (chay tren url trinh duyet)
        // Home : ten controller
        // Hello: method nam trong controller
        public string Goodbye()
        {
            return "See you again";
            //// https://localhost:port/Home/Goodbye
        }
    }
}
