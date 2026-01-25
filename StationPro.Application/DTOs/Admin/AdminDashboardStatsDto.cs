using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        // Existing properties...
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int FreePlanCount { get; set; }
        public int BasicPlanCount { get; set; }
        public int ProPlanCount { get; set; }
        public int EnterprisePlanCount { get; set; }

        // New properties for subscription management
        public int PendingSubscriptionsCount { get; set; }
        public int ApprovedTodayCount { get; set; }
        public int RejectedTodayCount { get; set; }
        public decimal PendingSubscriptionsTotal { get; set; }
    }

}
