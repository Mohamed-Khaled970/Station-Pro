using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs.Admin
{
    public class AdminDashboardStatsDto
    {
        public int TotalTenants { get; set; }
        public int ActiveTenants { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int FreePlanCount { get; set; }
        public int BasicPlanCount { get; set; }
        public int ProPlanCount { get; set; }
        public int EnterprisePlanCount { get; set; }
    }

}
