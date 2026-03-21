using Microsoft.EntityFrameworkCore;
using StationPro.Application.Contracts.Repositories;
using StationPro.Domain.Entities;
using StationPro.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Repositories
{
    public class DeviceRepository : Repository<Device>, IDeviceRepository
    {
        public DeviceRepository(ApplicationDbContext db) : base(db) { }

        /// <summary>
        /// All non-soft-deleted devices for the current tenant.
        /// TenantId scoping is handled by the global query filter in AppDbContext.
        /// </summary>
        public async Task<IEnumerable<Device>> GetAllActiveAsync()
            => await _set
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .ToListAsync();

        /// <summary>
        /// Devices not currently in use.
        /// </summary>
        public async Task<IEnumerable<Device>> GetAvailableAsync()
            => await _set
                .Where(d => d.IsActive && d.Status == DeviceStatus.Available)
                .OrderBy(d => d.Name)
                .ToListAsync();

        /// <summary>
        /// Single device with its active session pre-loaded.
        /// </summary>
        public async Task<Device?> GetWithActiveSessionAsync(int deviceId)
            => await _set
                .Include(d => d.Sessions.Where(s => s.Status == SessionStatus.Active))
                .FirstOrDefaultAsync(d => d.Id == deviceId);

        /// <summary>
        /// All devices with their active sessions loaded — for dashboard cards.
        /// </summary>
        public async Task<IEnumerable<Device>> GetAllWithActiveSessionsAsync()
            => await _set
                .Where(d => d.IsActive && d.Status == DeviceStatus.Available)
                .Include(d => d.Sessions.Where(s => s.Status == SessionStatus.Active))
                .OrderBy(d => d.Name)
                .ToListAsync();

        // Soft-delete: mark IsActive = false instead of removing the row.
        public override async Task DeleteAsync(int id)
        {
            var device = await GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Device {id} not found.");
            device.IsActive = false;
            await _db.SaveChangesAsync();
        }
    }
}
