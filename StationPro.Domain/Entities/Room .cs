using StationPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Domain.Entities
{
    public class Room : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool HasAC { get; set; }

        public decimal SingleHourlyRate { get; set; }
        public decimal MultiHourlyRate { get; set; }

        public int Capacity { get; set; } = 1;
        public int DeviceCount { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        // ── Computed helpers (not mapped to DB columns) ────────────────────────
        // EF Core translates IsOccupied/IsAvailable to NOT EXISTS(...) in SQL
        // when used inside a .Where() — no extra column needed.
        // When evaluated in memory (after .Include(r => r.Sessions)), they read
        // the already-loaded collection.
        public bool IsOccupied
            => Sessions.Any(s => s.Status == SessionStatus.Active);

        public bool IsAvailable
            => IsActive && !IsOccupied;

        // Convenience accessor — null when room is free.
        public Session? ActiveSession
            => Sessions.FirstOrDefault(s => s.Status == SessionStatus.Active);
    }
}
