using Microsoft.AspNetCore.Mvc;

namespace Station_Pro.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View("Register");
        }

        public IActionResult Login()
        {
            return View("Login");
        }
    }
}
