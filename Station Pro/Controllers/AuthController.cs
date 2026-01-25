using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs.Auth;

namespace Station_Pro.Controllers
{
    public class AuthController : Controller
    {
        private static  RegisterViewModel RegisterTestModel;
        public IActionResult Index()
        {
            return View();
        }

        // GET: Auth/Register
        public IActionResult Register()
        {
            return View("Register");
        }

        // POST: Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {


            try
            {
                // TODO: Replace with your actual registration logic
                // 1. Create user account
                // var user = new ApplicationUser { ... };
                // var result = await _userManager.CreateAsync(user, model.Password);

                // 2. Create tenant record
                // var tenant = new Tenant
                // {
                //     Name = model.StoreName,
                //     Email = model.Email,
                //     PhoneNumber = model.PhoneNumber,
                //     Subdomain = GenerateSubdomain(model.StoreName),
                //     Plan = SubscriptionPlan.Free,
                //     IsActive = true,
                //     CreatedDate = DateTime.Now
                // };
                // _context.Tenants.Add(tenant);
                // await _context.SaveChangesAsync();

                // 3. Sign in the user
                // await _signInManager.SignInAsync(user, isPersistent: false);
                RegisterTestModel = model;
                // For demo purposes, using a mock tenant ID
                int newTenantId = 1; // Replace with actual tenant.Id after creation

                TempData["Success"] = "Account created successfully! Please choose your subscription plan.";

                // 4. REDIRECT TO SUBSCRIPTION PAGE
                return RedirectToAction("Subscribe", "Subscription", new { tenantId = newTenantId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                return View("Register", model);
            }
        }

        //Helper Function for Test Only "Remove when implement the service and infrastructure layer"

        public static RegisterViewModel GetTenantModel()
        {
            return RegisterTestModel;
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View("Login");
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", model);
            }

            // TODO: Add your login logic
            // var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

            // if (result.Succeeded)
            // {
            //     return RedirectToAction("Index", "Dashboard");
            // }

            ModelState.AddModelError("", "Invalid login attempt");
            return View("Login", model);
        }

        // POST: Auth/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // Helper method to generate subdomain from store name
        private string GenerateSubdomain(string storeName)
        {
            // Convert to lowercase and replace spaces with hyphens
            var subdomain = storeName.ToLower()
                .Replace(" ", "-")
                .Replace("_", "-");

            // Remove special characters
            subdomain = new string(subdomain.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());

            // Add random suffix to ensure uniqueness
            subdomain += "-" + Guid.NewGuid().ToString().Substring(0, 6);

            return subdomain;
        }
    }
}
