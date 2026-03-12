using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Admin
{
    public class TenantAdminDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public SubscriptionPlan Plan { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime JoinedDate { get; set; }

        // Aggregated from related entities
        public int TotalDevices { get; set; }
        public int TotalSessions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
    }
}
