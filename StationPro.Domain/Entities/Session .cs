using StationPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Domain.Entities
{
    public class Session : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }

        // CHANGED: DeviceId is now nullable — room sessions have no device
        public int? DeviceId { get; set; }

        // NEW: room sessions reference a room instead
        public int? RoomId { get; set; }

        // NEW: discriminator — Device or Room
        public string SourceType { get; set; } = "";

        // NEW: Single or Multi
        public string SessionType { get; set; } = "";

        // NEW: customer data that needs to persist
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public int GuestCount { get; set; } = 1;

        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal HourlyRate { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Active;

        // NEW: payment method stored on the completed session
        public string? PaymentMethod { get; set; }

        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
        public Device? Device { get; set; }     // CHANGED: nullable
        public Room? Room { get; set; }         // NEW
    }
}
