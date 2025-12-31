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
        public decimal HourlyRate { get; set; }
        public int Capacity { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public Tenant Tenant { get; set; } = null!;
        public ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}
