using StationPro.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace StationPro.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool EmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public SubscriptionPlan Plan { get; set; } = SubscriptionPlan.Free;
        public DateTime? SubscriptionEndDate { get; set; }

        public ICollection<Device> Devices { get; set; } = new List<Device>();
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
