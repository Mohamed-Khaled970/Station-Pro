// =============================================================================
// FILE: Station_Pro/Controllers/AuthController.cs  (UPDATED)
//
// Uses a static in-memory tenant list (mirrors AdminController._tenants pattern).
// Register  → creates a Tenant object, adds to _tenants, saves to session.
// Login     → looks up tenant by email + password, saves to session,
//             routes to correct page based on subscription status.
// Logout    → clears session.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs.Auth;
using StationPro.Controllers;            // AdminController._subscriptionRequests
using StationPro.Domain.Entities;

namespace Station_Pro.Controllers
{
    public class AuthController : Controller
    {
        // =====================================================================
        // IN-MEMORY TENANT STORE
        // Same pattern as AdminController._tenants / _subscriptionRequests.
        // Replace with EF Core + repository when you wire up the real DB.
        // =====================================================================
        public static readonly List<Tenant> _tenants = new();
        private static int _nextTenantId = 10; // start at 10 to avoid clashing with admin sample data (ids 1-3)

        // Test helper — kept for SubscriptionController.GetTenantModel() calls
        private static RegisterViewModel? _lastRegisteredModel;
        public static RegisterViewModel? GetTenantModel() => _lastRegisteredModel;

        // Convenience lookup used by SubscriptionController to fill TenantName/Email
        public static Tenant? GetTenantById(int id)
            => _tenants.FirstOrDefault(t => t.Id == id);

        // =====================================================================

        public IActionResult Index() => View();

        // ── GET: Auth/Register ────────────────────────────────────────────────
        public IActionResult Register() => View("Register");

        // ── POST: Auth/Register ───────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            try
            {
                // Duplicate email check
                if (_tenants.Any(t => t.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("", "An account with this email already exists.");
                    return View("Register", model);
                }

                // Build the tenant object and add it to the in-memory list
                var tenant = new Tenant
                {
                    Id = ++_nextTenantId,
                    Name = model.StoreName,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    Plan = SubscriptionPlan.Free,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };

                _tenants.Add(tenant);
                _lastRegisteredModel = model;   // keep test helper in sync

                // Persist TenantId so SubscriptionGuardFilter can read it
                HttpContext.Session.SetInt32("TenantId", tenant.Id);

                await HttpContext.Session.CommitAsync();

                TempData["Success"] = "Account created! Please choose your subscription plan.";
                return RedirectToAction("Subscribe", "Subscription", new { tenantId = tenant.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                return View("Register", model);
            }
        }

        // ── GET: Auth/Login ───────────────────────────────────────────────────
        public IActionResult Login()
        {
            // Already logged in? redirect away from login page
            if (HttpContext.Session.GetInt32("TenantId").HasValue)
                return RedirectToAction("Index", "Dashboard");

            return View("Login");
        }

        // ── POST: Auth/Login ──────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            var tenant = _tenants.FirstOrDefault(t =>
                t.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

            if (tenant == null || !VerifyPassword(model.Password, tenant.PasswordHash))
                return Json(new { success = false, message = "Invalid email or password." });

            if (!tenant.IsActive)
                return Json(new { success = false, message = "Account deactivated. Contact support." });

            HttpContext.Session.SetInt32("TenantId", tenant.Id);

            // ✅ Force session to commit before responding
            await HttpContext.Session.CommitAsync();

            var subscription = AdminController._subscriptionRequests
                .Where(s => s.TenantId == tenant.Id)
                .OrderByDescending(s => s.SubmittedDate)
                .FirstOrDefault();

            var redirectUrl = subscription?.Status switch
            {
                "Pending" => $"/Subscription/Pending?tenantId={tenant.Id}",
                "Rejected" => $"/Subscription/Rejected?tenantId={tenant.Id}",
                "Approved" => "/Dashboard/Index",
                _ => $"/Subscription/Subscribe?tenantId={tenant.Id}"
            };

            return Json(new { success = true, redirectUrl });
        }

        // ── POST: Auth/Logout ─────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ── Private helpers ───────────────────────────────────────────────────


        /// <summary>
        /// Demo-only password hash.
        /// Replace with BCrypt or ASP.NET Identity PasswordHasher before production.
        /// </summary>
        private static string HashPassword(string password)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(bytes) + "_demo";
        }

        private static bool VerifyPassword(string password, string hash)
            => HashPassword(password) == hash;
    }
}