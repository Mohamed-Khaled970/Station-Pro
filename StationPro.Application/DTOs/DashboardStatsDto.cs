using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class DashboardStatsDto
    {
        public decimal TodayRevenue { get; set; }
        public int TotalSessions { get; set; }
        public int ActiveDevices { get; set; }
        public int TotalDevices { get; set; }
        public int ActiveSessions { get; set; }
        public decimal AverageSessionCost { get; set; }
    }
}
