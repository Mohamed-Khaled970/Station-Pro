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
using Station_Pro.Controllers;    // SubscriptionController.SyncApproval / SyncRejection

namespace StationPro.Controllers
{
    public class AdminController : Controller
    {
        // Simulate tenant data (replace with actual database queries)
        private static List<TenantAdminDto> _tenants = GenerateSampleTenants();

        // Shared subscription requests storage.
        // In production: use a database with proper repository pattern.
        public static List<PendingSubscriptionDto> _subscriptionRequests = new List<PendingSubscriptionDto>
        {
            new PendingSubscriptionDto
            {
                Id = 1,
                TenantId = 1,
                TenantName = "Game Zone Cairo",
                TenantEmail = "owner@gamezone.com",
                Subdomain = "gamezone",
                SubscriptionPlan = "Pro",
                Amount = 79,
                PaymentMethod = "VodafoneCash",
                PhoneNumber = "01001234567",
                TransactionReference = "VF123456789",
                PaymentProofUrl = "https://via.placeholder.com/400x600/3b82f6/ffffff?text=Payment+Proof+1",
                Notes = "Payment made on 20 Jan 2026",
                Status = "Pending",
                SubmittedDate = DateTime.Now.AddHours(-2),
                ReviewedDate = null,
                ReviewedBy = null,
                AdminNotes = null
            },
            new PendingSubscriptionDto
            {
                Id = 2,
                TenantId = 2,
                TenantName = "PlayStation Hub",
                TenantEmail = "admin@pshub.com",
                Subdomain = "pshub",
                SubscriptionPlan = "Basic",
                Amount = 29,
                PaymentMethod = "InstaPay",
                PhoneNumber = "01112345678",
                TransactionReference = "IP987654321",
                PaymentProofUrl = "https://via.placeholder.com/400x600/10b981/ffffff?text=Payment+Proof+2",
                Notes = null,
                Status = "Pending",
                SubmittedDate = DateTime.Now.AddHours(-5),
                ReviewedDate = null,
                ReviewedBy = null,
                AdminNotes = null
            }
        };

        // ── Dashboard ─────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            var stats = new AdminDashboardStatsDto
            {
                TotalTenants = _tenants.Count,
                ActiveTenants = _tenants.Count(t => t.IsActive),
                TotalRevenue = _tenants.Sum(t => t.TotalRevenue),
                MonthlyRevenue = _tenants.Sum(t => t.MonthlyRevenue),
                FreePlanCount = _tenants.Count(t => t.Plan == SubscriptionPlan.Free),
                BasicPlanCount = _tenants.Count(t => t.Plan == SubscriptionPlan.Basic),
                ProPlanCount = _tenants.Count(t => t.Plan == SubscriptionPlan.Pro),
                EnterprisePlanCount = _tenants.Count(t => t.Plan == SubscriptionPlan.Enterprise),
                PendingSubscriptionsCount = _subscriptionRequests.Count(s => s.Status == "Pending"),
                ApprovedTodayCount = _subscriptionRequests.Count(s =>
                                                s.Status == "Approved" &&
                                                s.ReviewedDate?.Date == DateTime.Today),
                RejectedTodayCount = _subscriptionRequests.Count(s =>
                                                s.Status == "Rejected" &&
                                                s.ReviewedDate?.Date == DateTime.Today),
                PendingSubscriptionsTotal = _subscriptionRequests
                                                .Where(s => s.Status == "Pending")
                                                .Sum(s => s.Amount)
            };

            return View(stats);
        }

        // ── Tenants partial ───────────────────────────────────────────────────
        public IActionResult Tenants()
        {
            return PartialView("_TenantsList", _tenants);
        }

        // ── Toggle tenant status ──────────────────────────────────────────────
        [HttpPost]
        public IActionResult ToggleTenantStatus(int tenantId)
        {
            var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound(new { success = false, message = "Tenant not found" });

            tenant.IsActive = !tenant.IsActive;
            return Ok(new { success = true, isActive = tenant.IsActive });
        }

        // ── Subscription request list ─────────────────────────────────────────
        // GET: /Admin/PendingSubscriptions?filter=all|Pending|Approved|Rejected
        public IActionResult PendingSubscriptions(string filter = "all")
        {
            var query = _subscriptionRequests.AsQueryable();

            if (!string.IsNullOrEmpty(filter) && filter != "all")
                query = query.Where(s =>
                    s.Status.Equals(filter, StringComparison.OrdinalIgnoreCase));

            return View(query.OrderByDescending(s => s.SubmittedDate).ToList());
        }

        // ── Approve ───────────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult ApproveSubscription(int id)
        {
            var subscription = _subscriptionRequests.FirstOrDefault(s => s.Id == id);

            if (subscription == null)
                return NotFound(new { success = false, message = "Subscription request not found" });

            if (subscription.Status != "Pending")
                return BadRequest(new { success = false, message = "This request has already been processed" });

            // Update the shared admin list
            subscription.Status = "Approved";
            subscription.ReviewedDate = DateTime.Now;
            subscription.ReviewedBy = "Admin User"; // Replace with HttpContext.User.Identity.Name

            // Update the tenant's plan in the tenant list
            var tenant = _tenants.FirstOrDefault(t => t.Id == subscription.TenantId);
            if (tenant != null && Enum.TryParse<SubscriptionPlan>(subscription.SubscriptionPlan, out var plan))
            {
                tenant.Plan = plan;
                tenant.SubscriptionEndDate = DateTime.Now.AddMonths(1);
                tenant.IsActive = true;
            }

            // ── NEW: keep SubscriptionController._store in sync ───────────────
            SubscriptionController.SyncApproval(subscription.TenantId, "Admin User");

            // TODO: send email notification to tenant

            return Ok(new { success = true, message = "Subscription approved successfully" });
        }

        // ── Reject ────────────────────────────────────────────────────────────
        [HttpPost]
        public IActionResult RejectSubscription(int id, [FromBody] RejectReasonDto dto)
        {
            var subscription = _subscriptionRequests.FirstOrDefault(s => s.Id == id);

            if (subscription == null)
                return NotFound(new { success = false, message = "Subscription request not found" });

            if (subscription.Status != "Pending")
                return BadRequest(new { success = false, message = "This request has already been processed" });

            if (string.IsNullOrWhiteSpace(dto.Reason))
                return BadRequest(new { success = false, message = "Please provide a reason for rejection" });

            // Update the shared admin list
            subscription.Status = "Rejected";
            subscription.ReviewedDate = DateTime.Now;
            subscription.ReviewedBy = "Admin User";
            subscription.AdminNotes = dto.Reason;   // rejection reason stored here

            // ── NEW: keep SubscriptionController._store in sync ───────────────
            SubscriptionController.SyncRejection(subscription.TenantId, dto.Reason, "Admin User");

            // TODO: send email notification with rejection reason

            return Ok(new { success = true, message = "Subscription rejected" });
        }

        // ── Pending count (real-time badge update) ────────────────────────────
        [HttpGet]
        public IActionResult GetPendingCount()
        {
            var count = _subscriptionRequests.Count(s => s.Status == "Pending");
            return Ok(new { count });
        }

        // ── Manual plan change (testing / admin override) ─────────────────────
        [HttpPost]
        public IActionResult UpdateSubscription(int tenantId, SubscriptionPlan plan)
        {
            var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
            if (tenant == null)
                return NotFound(new { success = false, message = "Tenant not found" });

            tenant.Plan = plan;
            tenant.SubscriptionEndDate = plan == SubscriptionPlan.Free
                ? null
                : DateTime.Now.AddMonths(1);

            return Ok(new { success = true, plan = plan.ToString() });
        }

        // ── Shared helper called by SubscriptionController ────────────────────
        public static void AddSubscriptionRequest(PendingSubscriptionDto subscription)
        {
            _subscriptionRequests.Add(subscription);
        }

        // ── Sample data ───────────────────────────────────────────────────────
        private static List<TenantAdminDto> GenerateSampleTenants()
        {
            return new List<TenantAdminDto>
            {
                new TenantAdminDto
                {
                    Id = 1, Name = "GameZone Cairo", Subdomain = "gamezone-cairo",
                    Email = "admin@gamezone-cairo.com", IsActive = true,
                    Plan = SubscriptionPlan.Pro,
                    SubscriptionEndDate = DateTime.Now.AddMonths(1),
                    TotalDevices = 12, TotalSessions = 450,
                    TotalRevenue = 125000m, MonthlyRevenue = 45000m,
                    JoinedDate = DateTime.Now.AddMonths(-6)
                },
                new TenantAdminDto
                {
                    Id = 2, Name = "PS Station Alex", Subdomain = "ps-station-alex",
                    Email = "owner@psstation.com", IsActive = true,
                    Plan = SubscriptionPlan.Basic,
                    SubscriptionEndDate = DateTime.Now.AddDays(15),
                    TotalDevices = 8, TotalSessions = 280,
                    TotalRevenue = 75000m, MonthlyRevenue = 28000m,
                    JoinedDate = DateTime.Now.AddMonths(-4)
                },
                new TenantAdminDto
                {
                    Id = 3, Name = "Ultimate Gaming Hub", Subdomain = "ultimate-gaming",
                    Email = "info@ultimategaming.com", IsActive = true,
                    Plan = SubscriptionPlan.Enterprise,
                    SubscriptionEndDate = DateTime.Now.AddMonths(3),
                    TotalDevices = 25, TotalSessions = 890,
                    TotalRevenue = 310000m, MonthlyRevenue = 95000m,
                    JoinedDate = DateTime.Now.AddMonths(-12)
                }
            };
        }
    }
}