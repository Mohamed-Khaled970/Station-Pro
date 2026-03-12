using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Repositories;
using StationPro.Application.DTOs.Admin;
using StationPro.Domain.Entities;
using StationPro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Repositories
{
    public class AdminTenantRepository : IAdminTenantRepository
    {
        private readonly ApplicationDbContext _db;

        public AdminTenantRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        // ── All tenants with aggregated stats ─────────────────────────────────
        public async Task<List<TenantAdminDto>> GetAllTenantsAsync()
        {
            var today = DateTime.UtcNow.Date;

            return await _db.Tenants
                .IgnoreQueryFilters()         // bypass global TenantId filter
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TenantAdminDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    IsActive = t.IsActive,
                    Plan = t.Plan,
                    SubscriptionEndDate = t.SubscriptionEndDate,
                    JoinedDate = t.CreatedAt,
                    TotalDevices = t.Devices.Count(d => d.IsActive),
                    TotalSessions = t.Sessions.Count(),
                    // Sum all completed session costs as "total revenue"
                    TotalRevenue = t.Sessions
                                          .Where(s => s.Status == SessionStatus.Completed)
                                          .Sum(s => (decimal?)s.TotalCost) ?? 0m,
                    // This month only
                    MonthlyRevenue = t.Sessions
                                          .Where(s => s.Status == SessionStatus.Completed
                                                   && s.StartTime.Month == today.Month
                                                   && s.StartTime.Year == today.Year)
                                          .Sum(s => (decimal?)s.TotalCost) ?? 0m,
                })
                .ToListAsync();
        }

        // ── Single tenant admin view ──────────────────────────────────────────
        public async Task<TenantAdminDto?> GetTenantAdminDtoAsync(int tenantId)
        {
            var today = DateTime.UtcNow.Date;

            return await _db.Tenants
                .IgnoreQueryFilters()
                .Where(t => t.Id == tenantId)
                .Select(t => new TenantAdminDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Email = t.Email,
                    PhoneNumber = t.PhoneNumber,
                    IsActive = t.IsActive,
                    Plan = t.Plan,
                    SubscriptionEndDate = t.SubscriptionEndDate,
                    JoinedDate = t.CreatedAt,
                    TotalDevices = t.Devices.Count(d => d.IsActive),
                    TotalSessions = t.Sessions.Count(),
                    TotalRevenue = t.Sessions
                                          .Where(s => s.Status == SessionStatus.Completed)
                                          .Sum(s => (decimal?)s.TotalCost) ?? 0m,
                    MonthlyRevenue = t.Sessions
                                          .Where(s => s.Status == SessionStatus.Completed
                                                   && s.StartTime.Month == today.Month
                                                   && s.StartTime.Year == today.Year)
                                          .Sum(s => (decimal?)s.TotalCost) ?? 0m,
                })
                .FirstOrDefaultAsync();
        }

        // ── Toggle IsActive ───────────────────────────────────────────────────
        public async Task<bool> ToggleTenantStatusAsync(int tenantId)
        {
            var tenant = await _db.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null) return false;

            tenant.IsActive = !tenant.IsActive;
            tenant.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        // ── Admin plan override (without a subscription request) ──────────────
        public async Task<bool> UpdateTenantPlanAsync(int tenantId, SubscriptionPlan plan)
        {
            var tenant = await _db.Tenants
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null) return false;

            tenant.Plan = plan;
            tenant.IsActive = plan != SubscriptionPlan.Free;
            tenant.SubscriptionEndDate = plan == SubscriptionPlan.Free
                ? null
                : DateTime.UtcNow.AddMonths(1);
            tenant.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        // ── Dashboard stats (one DB round-trip) ───────────────────────────────
        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.UtcNow.Date;

            // Tenant plan counts
            var planCounts = await _db.Tenants
                .IgnoreQueryFilters()
                .GroupBy(t => t.Plan)
                .Select(g => new { Plan = g.Key, Count = g.Count() })
                .ToListAsync();

            int GetCount(SubscriptionPlan p) =>
                planCounts.FirstOrDefault(x => x.Plan == p)?.Count ?? 0;

            // Subscription request stats
            var subStats = await _db.SubscriptionRequests
                .GroupBy(s => s.Status)
                .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var pendingSubs = subStats.FirstOrDefault(x => x.Status == SubscriptionRequestStatus.Pending);
            var approvedToday = await _db.SubscriptionRequests
                .CountAsync(s => s.Status == SubscriptionRequestStatus.Approved
                              && s.ReviewedDate != null
                              && s.ReviewedDate.Value.Date == today);
            var rejectedToday = await _db.SubscriptionRequests
                .CountAsync(s => s.Status == SubscriptionRequestStatus.Rejected
                              && s.ReviewedDate != null
                              && s.ReviewedDate.Value.Date == today);

            // Revenue from sessions
            var revenueStats = await _db.Sessions
                .IgnoreQueryFilters()
                .Where(s => s.Status == SessionStatus.Completed)
                .GroupBy(s => 1)
                .Select(g => new
                {
                    Total = g.Sum(s => s.TotalCost),
                    Monthly = g.Where(s => s.StartTime.Month == today.Month
                                        && s.StartTime.Year == today.Year)
                               .Sum(s => s.TotalCost)
                })
                .FirstOrDefaultAsync();

            var allTenants = await _db.Tenants.IgnoreQueryFilters().ToListAsync();

            return new AdminDashboardStatsDto
            {
                TotalTenants = allTenants.Count,
                ActiveTenants = allTenants.Count(t => t.IsActive),
                TotalRevenue = revenueStats?.Total ?? 0m,
                MonthlyRevenue = revenueStats?.Monthly ?? 0m,
                FreePlanCount = GetCount(SubscriptionPlan.Free),
                BasicPlanCount = GetCount(SubscriptionPlan.Basic),
                ProPlanCount = GetCount(SubscriptionPlan.Pro),
                EnterprisePlanCount = GetCount(SubscriptionPlan.Enterprise),
                PendingSubscriptionsCount = pendingSubs?.Count ?? 0,
                ApprovedTodayCount = approvedToday,
                RejectedTodayCount = rejectedToday,
                PendingSubscriptionsTotal = pendingSubs?.Total ?? 0m,
            };
        }
    }
}
