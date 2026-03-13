using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Repositories;
using StationPro.Application.DTOs;
using StationPro.Domain.Entities;
using StationPro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Repositories
{
    public class SessionRepository : Repository<Session>, ISessionRepository
    {
        public SessionRepository(ApplicationDbContext db) : base(db) { }

        /// <summary>
        /// All sessions for the current tenant, optionally filtered by date range.
        /// TenantId scoping is handled by the global query filter in AppDbContext.
        /// </summary>
        public async Task<IEnumerable<Session>> GetAllAsync(
            DateTime? from = null, DateTime? to = null)
        {
            var q = _set.AsQueryable();
            if (from.HasValue) q = q.Where(s => s.StartTime >= from.Value);
            if (to.HasValue) q = q.Where(s => s.StartTime <= to.Value);
            return await q.OrderByDescending(s => s.StartTime).ToListAsync();
        }

        /// <summary>Only Active sessions for the current tenant.</summary>
        public async Task<IEnumerable<Session>> GetActiveAsync()
            => await _set
                .Where(s => s.Status == SessionStatus.Active)
                .Include(s => s.Device)
                .Include(s => s.Room)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

        /// <summary>Active sessions for a specific device.</summary>
        public async Task<IEnumerable<Session>> GetActiveForDeviceAsync(int deviceId)
            => await _set
                .Where(s => s.DeviceId == deviceId && s.Status == SessionStatus.Active)
                .ToListAsync();

        /// <summary>Active sessions for a specific room.</summary>
        public async Task<IEnumerable<Session>> GetActiveForRoomAsync(int roomId)
            => await _set
                .Where(s => s.RoomId == roomId && s.Status == SessionStatus.Active)
                .ToListAsync();

        /// <summary>Paged + filtered result set for the current tenant.</summary>
        public async Task<(IEnumerable<Session> Items, int Total)> GetPagedAsync(
            SessionFilterRequest filter)
        {
            var q = _set.AsQueryable();

            // Date filter
            q = filter.DateFilter switch
            {
                "today" => q.Where(s => s.StartTime.Date == DateTime.Today),
                "yesterday" => q.Where(s => s.StartTime.Date == DateTime.Today.AddDays(-1)),
                "week" => q.Where(s => s.StartTime >= DateTime.Today.AddDays(-7)),
                "month" => q.Where(s => s.StartTime >= DateTime.Today.AddMonths(-1)),
                _ => q
            };

            // Status filter
            if (filter.Status != "all")
            {
                var target = filter.Status.ToLower() == "active"
                    ? SessionStatus.Active
                    : SessionStatus.Completed;
                q = q.Where(s => s.Status == target);
            }

            // Device filter
            if (filter.DeviceId.HasValue)
                q = q.Where(s => s.DeviceId == filter.DeviceId);

            // Search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim().ToLower();
                q = q.Where(s =>
                    (s.CustomerName != null && s.CustomerName.ToLower().Contains(term)) ||
                    (s.Device != null && s.Device.Name.ToLower().Contains(term)) ||
                    (s.Room != null && s.Room.Name.ToLower().Contains(term)));
            }

            var total = await q.CountAsync();

            var items = await q
                .Include(s => s.Device)
                .Include(s => s.Room)
                .OrderByDescending(s => s.StartTime)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, total);
        }

        /// <summary>Revenue sum for the current tenant on a given calendar day.</summary>
        public async Task<decimal> GetDailyRevenueAsync(DateTime date)
        {
            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            // Completed sessions: stored TotalCost
            var completedRevenue = await _set
                .Where(s => s.Status == SessionStatus.Completed
                         && s.StartTime >= dayStart
                         && s.StartTime < dayEnd)
                .SumAsync(s => s.TotalCost);

            // Active sessions: calculate running cost in memory
            var activeSessions = await _set
                .Where(s => s.Status == SessionStatus.Active
                         && s.StartTime >= dayStart
                         && s.StartTime < dayEnd)
                .ToListAsync();

            var activeRevenue = activeSessions.Sum(s =>
                Math.Round((decimal)(DateTime.UtcNow - s.StartTime).TotalHours * s.HourlyRate, 2));

            return completedRevenue + activeRevenue;
        }
    }
}
