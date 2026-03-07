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

        // CHANGED: was a single HourlyRate — rooms need two rates
        public decimal SingleHourlyRate { get; set; }
        public decimal MultiHourlyRate { get; set; }

        public int Capacity { get; set; } = 1;
        public int DeviceCount { get; set; } = 0;   // how many devices are in this room
        public bool IsActive { get; set; } = true;

        // Navigation
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();  // NEW
    }
}
