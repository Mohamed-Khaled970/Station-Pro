using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs.Admin;
using StationPro.Application.DTOs.Subscriptions;
using StationPro.Domain.Entities;

namespace StationPro.Controllers
{
    public class AdminController : Controller
    {
        // Simulate tenant data (replace with actual database queries)
        private static List<TenantAdminDto> _tenants = GenerateSampleTenants();

        // Shared subscription requests storage
        // In production, this should be in a database with proper repository pattern
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

        // Dashboard
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
                ApprovedTodayCount = _subscriptionRequests.Count(s => s.Status == "Approved" && s.ReviewedDate?.Date == DateTime.Today),
                RejectedTodayCount = _subscriptionRequests.Count(s => s.Status == "Rejected" && s.ReviewedDate?.Date == DateTime.Today),
                PendingSubscriptionsTotal = _subscriptionRequests.Where(s => s.Status == "Pending").Sum(s => s.Amount)
            };

            return View(stats);
        }

        // Tenants List (Partial View)
        public IActionResult Tenants()
        {
            return PartialView("_TenantsList", _tenants);
        }

        // Toggle Tenant Status
        [HttpPost]
        public IActionResult ToggleTenantStatus(int tenantId)
        {
            var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound(new { success = false, message = "Tenant not found" });
            }

            tenant.IsActive = !tenant.IsActive;
            return Ok(new { success = true, isActive = tenant.IsActive });
        }

        // GET: Admin/PendingSubscriptions
        public IActionResult PendingSubscriptions(string filter = "all")
        {
            var subscriptions = _subscriptionRequests.AsQueryable();

            if (filter != "all" && filter != null)
            {
                subscriptions = subscriptions.Where(s => s.Status.Equals(filter, StringComparison.OrdinalIgnoreCase));
            }

            return View(subscriptions.OrderByDescending(s => s.SubmittedDate).ToList());
        }

        // POST: Admin/ApproveSubscription/5
        [HttpPost]
        public IActionResult ApproveSubscription(int id)
        {
            var subscription = _subscriptionRequests.FirstOrDefault(s => s.Id == id);

            if (subscription == null)
            {
                return NotFound(new { success = false, message = "Subscription request not found" });
            }

            if (subscription.Status != "Pending")
            {
                return BadRequest(new { success = false, message = "This request has already been processed" });
            }

            // Update subscription status
            subscription.Status = "Approved";
            subscription.ReviewedDate = DateTime.Now;
            subscription.ReviewedBy = "Admin User"; // In real app, get from current authenticated user

            // Update tenant's subscription plan
            var tenant = _tenants.FirstOrDefault(t => t.Id == subscription.TenantId);
            if (tenant != null)
            {
                // Parse the plan from string
                if (Enum.TryParse<SubscriptionPlan>(subscription.SubscriptionPlan, out var plan))
                {
                    tenant.Plan = plan;
                    tenant.SubscriptionEndDate = DateTime.Now.AddMonths(1);
                    tenant.IsActive = true;
                }
            }

            // In real implementation:
            // 1. Update database
            // 2. Send email notification to tenant
            // 3. Log the action
            // 4. Maybe trigger webhook or other integrations

            return Ok(new { success = true, message = "Subscription approved successfully" });
        }

        // POST: Admin/RejectSubscription/5
        [HttpPost]
        public IActionResult RejectSubscription(int id, [FromBody] RejectReasonDto dto)
        {
            var subscription = _subscriptionRequests.FirstOrDefault(s => s.Id == id);

            if (subscription == null)
            {
                return NotFound(new { success = false, message = "Subscription request not found" });
            }

            if (subscription.Status != "Pending")
            {
                return BadRequest(new { success = false, message = "This request has already been processed" });
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                return BadRequest(new { success = false, message = "Please provide a reason for rejection" });
            }

            // Update subscription status
            subscription.Status = "Rejected";
            subscription.ReviewedDate = DateTime.Now;
            subscription.ReviewedBy = "Admin User"; // In real app, get from current authenticated user
            subscription.AdminNotes = dto.Reason;

            // In real implementation:
            // 1. Update database
            // 2. Send email notification to tenant with rejection reason
            // 3. Log the action

            return Ok(new { success = true, message = "Subscription rejected" });
        }

        // GET: Admin/GetPendingCount - For real-time updates
        [HttpGet]
        public IActionResult GetPendingCount()
        {
            var count = _subscriptionRequests.Count(s => s.Status == "Pending");
            return Ok(new { count });
        }

        // Manual subscription update (for testing)
        [HttpPost]
        public IActionResult UpdateSubscription(int tenantId, SubscriptionPlan plan)
        {
            var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
            if (tenant == null)
            {
                return NotFound(new { success = false, message = "Tenant not found" });
            }

            tenant.Plan = plan;
            tenant.SubscriptionEndDate = plan == SubscriptionPlan.Free
                ? null
                : DateTime.Now.AddMonths(1);

            return Ok(new { success = true, plan = plan.ToString() });
        }

        // Helper method to add subscription request (for integration with SubscriptionController)
        public static void AddSubscriptionRequest(PendingSubscriptionDto subscription)
        {
            _subscriptionRequests.Add(subscription);
        }

        private static List<TenantAdminDto> GenerateSampleTenants()
        {
            return new List<TenantAdminDto>
            {
                new TenantAdminDto
                {
                    Id = 1,
                    Name = "GameZone Cairo",
                    Subdomain = "gamezone-cairo",
                    Email = "admin@gamezone-cairo.com",
                    IsActive = true,
                    Plan = SubscriptionPlan.Pro,
                    SubscriptionEndDate = DateTime.Now.AddMonths(1),
                    TotalDevices = 12,
                    TotalSessions = 450,
                    TotalRevenue = 125000m,
                    MonthlyRevenue = 45000m,
                    JoinedDate = DateTime.Now.AddMonths(-6)
                },
                new TenantAdminDto
                {
                    Id = 2,
                    Name = "PS Station Alex",
                    Subdomain = "ps-station-alex",
                    Email = "owner@psstation.com",
                    IsActive = true,
                    Plan = SubscriptionPlan.Basic,
                    SubscriptionEndDate = DateTime.Now.AddDays(15),
                    TotalDevices = 8,
                    TotalSessions = 280,
                    TotalRevenue = 75000m,
                    MonthlyRevenue = 28000m,
                    JoinedDate = DateTime.Now.AddMonths(-4)
                },
                new TenantAdminDto
                {
                    Id = 3,
                    Name = "Ultimate Gaming Hub",
                    Subdomain = "ultimate-gaming",
                    Email = "info@ultimategaming.com",
                    IsActive = true,
                    Plan = SubscriptionPlan.Enterprise,
                    SubscriptionEndDate = DateTime.Now.AddMonths(3),
                    TotalDevices = 25,
                    TotalSessions = 890,
                    TotalRevenue = 310000m,
                    MonthlyRevenue = 95000m,
                    JoinedDate = DateTime.Now.AddMonths(-12)
                }
            };
        }
    }

    public class RejectReasonDto
    {
        public string Reason { get; set; } = string.Empty;
    }
}