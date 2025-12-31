using StationPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace StationPro.Domain.Entities
{
    public class Device : BaseEntity, ITenantEntity
    {
        public int TenantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public decimal HourlyRate { get; set; }
        public int? RoomId { get; set; }
        public bool IsActive { get; set; } = true;
        public DeviceStatus Status { get; set; } = DeviceStatus.Available;
        // Navigation Properties
        public Tenant Tenant { get; set; } = null!;
        public Room? Room { get; set; }
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
