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
    public class RoomRepository : Repository<Room>, IRoomRepository
    {
        public RoomRepository(ApplicationDbContext db) : base(db) { }

        /// <summary>
        /// All active rooms for the current tenant.
        /// TenantId scoping is handled by the global query filter in AppDbContext.
        /// </summary>
        public async Task<IEnumerable<Room>> GetAllActiveAsync()
            => await _set
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();

        /// <summary>
        /// Rooms with no active session (available to book).
        /// EF Core translates !Sessions.Any(...) → NOT EXISTS subquery in SQL.
        /// </summary>
        public async Task<IEnumerable<Room>> GetAvailableAsync()
            => await _set
                .Where(r => r.IsActive &&
                            !r.Sessions.Any(s => s.Status == SessionStatus.Active))
                .OrderBy(r => r.Name)
                .ToListAsync();

        /// <summary>
        /// Single room with its active session pre-loaded so IsOccupied,
        /// IsAvailable, and ActiveSession work correctly in memory.
        /// </summary>
        public async Task<Room?> GetWithActiveSessionAsync(int roomId)
            => await _set
                .Include(r => r.Sessions.Where(s => s.Status == SessionStatus.Active))
                .FirstOrDefaultAsync(r => r.Id == roomId);

        /// <summary>
        /// All rooms with their active sessions loaded — useful for the dashboard
        /// device/room cards where you need runtime status for every room at once.
        /// </summary>
        public async Task<IEnumerable<Room>> GetAllWithActiveSessionsAsync()
            => await _set
                .Where(r => r.IsActive)
                .Include(r => r.Sessions.Where(s => s.Status == SessionStatus.Active))
                .OrderBy(r => r.Name)
                .ToListAsync();
    }
}
