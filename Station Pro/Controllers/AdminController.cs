// =============================================================================
// FILE: StationPro/Controllers/AdminController.cs  (UPDATED)
//
// Changes vs original:
//  1. ApproveSubscription  → calls SubscriptionController.SyncApproval()
//  2. RejectSubscription   → calls SubscriptionController.SyncRejection()
//  3. Local RejectReasonDto removed — use the one from SubscriptionDtos.cs
//
// Everything else (dashboard stats, tenant management, sample data) is
// preserved exactly as it was in the original file.
// =============================================================================

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs.Admin;
using StationPro.Application.DTOs.Subscriptions;
using StationPro.Domain.Entities;
using Station_Pro.Controllers;
using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;    // SubscriptionController.SyncApproval / SyncRejection

namespace StationPro.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminTenantRepository _adminRepo;
        private readonly ISubscriptionRequestService _subService;

        public AdminController(
            IAdminTenantRepository adminRepo,
            ISubscriptionRequestService subService)
        {
            _adminRepo = adminRepo;
            _subService = subService;
        }

        // ── Dashboard ─────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var stats = await _adminRepo.GetDashboardStatsAsync();
            return View(stats);
        }

        // ── Tenants partial (HTMX) ────────────────────────────────────────────
        public async Task<IActionResult> Tenants()
        {
            var tenants = await _adminRepo.GetAllTenantsAsync();
            return PartialView("_TenantsList", tenants);
        }

        // ── Toggle tenant active status ───────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> ToggleTenantStatus(int tenantId)
        {
            var ok = await _adminRepo.ToggleTenantStatusAsync(tenantId);

            if (!ok)
                return NotFound(new { success = false, message = "Tenant not found." });

            return Ok(new { success = true });
        }

        // ── Subscription request list ─────────────────────────────────────────
        // GET /Admin/PendingSubscriptions?filter=all|Pending|Approved|Rejected
        public async Task<IActionResult> PendingSubscriptions(string filter = "all")
        {
            var all = await _subService.GetAllAsync();

            // Map domain entities → DTOs for the view
            var dtos = all
                .Where(r => filter == "all" ||
                            r.Status.ToString().Equals(filter, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(r => r.SubmittedDate)
                .Select(r => new PendingSubscriptionDto
                {
                    Id = r.Id,
                    TenantId = r.TenantId,
                    TenantName = r.Tenant?.Name ?? $"Tenant #{r.TenantId}",
                    TenantEmail = r.Tenant?.Email ?? string.Empty,
                    SubscriptionPlan = r.SubscriptionPlan.ToString(),
                    Amount = r.Amount,
                    PaymentMethod = r.PaymentMethod,
                    PhoneNumber = r.PhoneNumber,
                    TransactionReference = r.TransactionReference,
                    PaymentProofUrl = r.PaymentProofPath,
                    Notes = r.Notes,
                    AdminNotes = r.AdminNotes,
                    Status = r.Status.ToString(),
                    SubmittedDate = r.SubmittedDate,
                    ReviewedDate = r.ReviewedDate,
                    ReviewedBy = r.ReviewedByUserId,
                })
                .ToList();

            return View(dtos);
        }

        // ── Approve ───────────────────────────────────────────────────────────
        // POST /Admin/ApproveSubscription?id=5
        [HttpPost]
        public async Task<IActionResult> ApproveSubscription(int id)
        {
            var reviewedBy = HttpContext.Session.GetString("AdminName") ?? "Admin";

            var (success, error) = await _subService.ApproveAsync(id, reviewedBy);

            if (!success)
                return BadRequest(new { success = false, message = error });

            return Ok(new { success = true, message = "Subscription approved and tenant activated." });
        }

        // ── Reject ────────────────────────────────────────────────────────────
        // POST /Admin/RejectSubscription?id=5   body: { "reason": "..." }
        [HttpPost]
        public async Task<IActionResult> RejectSubscription(int id, [FromBody] RejectReasonDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Reason))
                return BadRequest(new { success = false, message = "Please provide a reason." });

            var reviewedBy = HttpContext.Session.GetString("AdminName") ?? "Admin";

            var (success, error) = await _subService.RejectAsync(id, dto.Reason, reviewedBy);

            if (!success)
                return BadRequest(new { success = false, message = error });

            return Ok(new { success = true, message = "Subscription rejected." });
        }

        // ── Pending count (polled by JS badge) ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPendingCount()
        {
            var all = await _subService.GetAllAsync();
            var pending = all.Count(r => r.Status == SubscriptionRequestStatus.Pending);
            return Ok(new { count = pending });
        }

        // ── Admin plan override (bypass subscription flow) ────────────────────
        [HttpPost]
        public async Task<IActionResult> UpdateSubscription(int tenantId, SubscriptionPlan plan)
        {
            var ok = await _adminRepo.UpdateTenantPlanAsync(tenantId, plan);

            if (!ok)
                return NotFound(new { success = false, message = "Tenant not found." });

            return Ok(new { success = true, plan = plan.ToString() });
        }


        [HttpGet]
        public async Task<IActionResult> TenantDetails(int tenantId)
        {
            var tenant = await _adminRepo.GetTenantAdminDtoAsync(tenantId);

            if (tenant == null)
                return NotFound(new { success = false, message = "Tenant not found." });

            return Ok(new
            {
                success = true,
                tenant = new
                {
                    name = tenant.Name,
                    email = tenant.Email,
                    phoneNumber = tenant.PhoneNumber,
                    plan = tenant.Plan.ToString(),
                    isActive = tenant.IsActive,
                    joinedDate = tenant.JoinedDate.ToString("MMM dd, yyyy"),
                    subscriptionEndDate = tenant.SubscriptionEndDate?.ToString("MMM dd, yyyy"),
                    totalDevices = tenant.TotalDevices,
                    totalSessions = tenant.TotalSessions,
                    totalRevenue = tenant.TotalRevenue,
                    monthlyRevenue = tenant.MonthlyRevenue,
                }
            });
        }
    }
}