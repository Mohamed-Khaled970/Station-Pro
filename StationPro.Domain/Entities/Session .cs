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
        public int DeviceId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal HourlyRate { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Active;
        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;

        // Navigation Properties
        public Tenant Tenant { get; set; } = null!;
        public Device Device { get; set; } = null!;

    }
}
