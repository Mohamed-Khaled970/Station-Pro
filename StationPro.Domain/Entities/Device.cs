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

        // Single Session Rate (default/required)
        public decimal SingleSessionRate { get; set; }

        // Multi Session Rate (optional - only for applicable devices)
        public decimal? MultiSessionRate { get; set; }

        // Whether this device supports multi-session mode
        public bool SupportsMultiSession { get; set; } = false;

        public int? RoomId { get; set; }
        public bool IsActive { get; set; } = true;
        public DeviceStatus Status { get; set; } = DeviceStatus.Available;

        // Navigation Properties
        public Tenant Tenant { get; set; } = null!;
        public Room? Room { get; set; }
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

        // Helper method to check if device type typically supports multi-session
        public bool IsMultiSessionCapable()
        {
            return Type == DeviceType.PS5 ||
                   Type == DeviceType.PS4 ||
                   Type == DeviceType.PS3 ||
                   Type == DeviceType.Xbox ||
                   Type == DeviceType.PingPong ||
                   Type == DeviceType.Pool ||
                   Type == DeviceType.Billiards;
        }
    }
}
