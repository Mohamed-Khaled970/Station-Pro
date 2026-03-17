// =============================================================================
// FILE: Station_Pro/Controllers/AuthController.cs
// =============================================================================

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using StationPro.Application.Contracts.Services;
using StationPro.Application.DTOs.Auth;
using StationPro.Domain.Entities;
using System.Security.Claims;

namespace Station_Pro.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _auth;
        private readonly ISubscriptionRequestService _subscriptionRequest;
        private readonly IAdminService _adminService;

        public AuthController(IAuthService auth, ISubscriptionRequestService subscriptionRequest, IAdminService adminService)
        {
            _auth = auth;
            _subscriptionRequest = subscriptionRequest;
            _adminService = adminService;
        }

        // ── Register ──────────────────────────────────────────────────────────
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!model.AcceptTerms)
                ModelState.AddModelError(nameof(model.AcceptTerms), "You must accept the terms and conditions.");

            if (!ModelState.IsValid)
                return View(model);

            var (success, tenantId, error) = await _auth.RegisterAsync(
                model.StoreName,
                model.Email,
                model.PhoneNumber,
                model.Password);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, error ?? "Registration failed. Please try again.");
                return View(model);
            }

            // ── Sign in as tenant immediately after registration ──────────────
            await SignInAsTenantAsync(tenantId);

            TempData["Success"] = "Account created! Please choose your subscription plan.";
            return RedirectToAction("Subscribe", "Subscription", new { tenantId });
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public IActionResult Login()
        {
            // If already authenticated as a tenant, skip login page
            var tenantClaim = User.FindFirst("TenantId");
            if (tenantClaim != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            // ── Check admin first ─────────────────────────────────────────────
            var admin = await _adminService.TryToGetAdmin(model.Email);

            if (admin != null && BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            {
                await SignInAsAdminAsync(admin.Id, admin.Name);
                return Json(new { success = true, redirectUrl = "/Admin/Index" });
            }

            // ── Check tenant ──────────────────────────────────────────────────
            var (success, tenantId, error) = await _auth.LoginAsync(model.Email, model.Password);

            if (!success)
                return Json(new { success = false, message = error });

            await SignInAsTenantAsync(tenantId);

            var latestRequest = await _subscriptionRequest.GetLatestRequest(tenantId);

            if (latestRequest == null)
                return Json(new { success = true, redirectUrl = $"/Subscription/Subscribe?tenantId={tenantId}" });

            var redirectUrl = latestRequest.Status switch
            {
                SubscriptionRequestStatus.Pending => $"/Subscription/Pending?tenantId={tenantId}",
                SubscriptionRequestStatus.Rejected => $"/Subscription/Rejected?tenantId={tenantId}",
                SubscriptionRequestStatus.Approved => "/Dashboard/Index",
                _ => $"/Subscription/Subscribe?tenantId={tenantId}"
            };

            return Json(new { success = true, redirectUrl });
        }

        // ── Logout ────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Deactivated() => View();

        // ── Private helpers ───────────────────────────────────────────────────

        private async Task SignInAsTenantAsync(int tenantId)
        {
            var claims = new List<Claim>
            {
                new Claim("TenantId", tenantId.ToString()),
                new Claim("Role", "Tenant"),
                new Claim(ClaimTypes.NameIdentifier, tenantId.ToString())
            };

            await SignInWithClaimsAsync(claims);
        }

        private async Task SignInAsAdminAsync(int adminId, string adminName)
        {
            var claims = new List<Claim>
            {
                new Claim("AdminId",  adminId.ToString()),
                new Claim("AdminName", adminName),
                new Claim("Role", "Admin"),
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                new Claim(ClaimTypes.Name, adminName)
            };

            await SignInWithClaimsAsync(claims);
        }

        private async Task SignInWithClaimsAsync(List<Claim> claims)
        {
            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,                       // survives browser close
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });
        }
    }
}