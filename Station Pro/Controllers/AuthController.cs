// =============================================================================
// FILE: Station_Pro/Controllers/AuthController.cs
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.Contracts.Services;
using StationPro.Application.DTOs.Auth;
using StationPro.Domain.Entities;

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
            // Manual check for AcceptTerms — checkbox binding is unreliable with attributes
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

            HttpContext.Session.SetInt32("TenantId", tenantId);
            await HttpContext.Session.CommitAsync();

            TempData["Success"] = "Account created! Please choose your subscription plan.";
            return RedirectToAction("Subscribe", "Subscription", new { tenantId });
        }

        // ── Login ─────────────────────────────────────────────────────────────
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("TenantId").HasValue)
                return RedirectToAction("Index", "Dashboard");
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Invalid input." });

            var admin = await _adminService.TryToGetAdmin(model.Email);

            if (admin != null && BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            {
                HttpContext.Session.SetInt32("AdminId", admin.Id);
                HttpContext.Session.SetString("AdminName", admin.Name);
                await HttpContext.Session.CommitAsync();
                return Json(new { success = true, redirectUrl = "/Admin/Index" });
            }

            var (success, tenantId, error) = await _auth.LoginAsync(model.Email, model.Password);

            if (!success)
                return Json(new { success = false, message = error });

            HttpContext.Session.SetInt32("TenantId", tenantId);
            await HttpContext.Session.CommitAsync();

            var latestRequest = await _subscriptionRequest.GetLatestRequest(tenantId);

            var redirectUrl = latestRequest?.Status switch
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
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}