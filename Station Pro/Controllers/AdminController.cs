using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs.Admin;
using StationPro.Domain.Entities;

namespace StationPro.Controllers
{
    public class AdminController : Controller
    {
        // Simulate tenant data (replace with actual database queries)
        private static List<TenantAdminDto> _tenants = GenerateSampleTenants();

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
                EnterprisePlanCount = _tenants.Count(t => t.Plan == SubscriptionPlan.Enterprise)
            };

            return View(stats);
        }

        public IActionResult Tenants()
        {
            return PartialView("_TenantsList", _tenants);
        }

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
                },
                new TenantAdminDto
                {
                    Id = 4,
                    Name = "Casual Gamers",
                    Subdomain = "casual-gamers",
                    Email = "hello@casualgamers.com",
                    IsActive = true,
                    Plan = SubscriptionPlan.Free,
                    SubscriptionEndDate = null,
                    TotalDevices = 3,
                    TotalSessions = 85,
                    TotalRevenue = 12000m,
                    MonthlyRevenue = 4000m,
                    JoinedDate = DateTime.Now.AddMonths(-1)
                },
                new TenantAdminDto
                {
                    Id = 5,
                    Name = "Esports Arena",
                    Subdomain = "esports-arena",
                    Email = "contact@esportsarena.com",
                    IsActive = false,
                    Plan = SubscriptionPlan.Pro,
                    SubscriptionEndDate = DateTime.Now.AddDays(-5),
                    TotalDevices = 15,
                    TotalSessions = 520,
                    TotalRevenue = 185000m,
                    MonthlyRevenue = 0m,
                    JoinedDate = DateTime.Now.AddMonths(-8)
                }
            };
        }
    }
}
