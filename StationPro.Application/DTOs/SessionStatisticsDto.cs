using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class SessionStatisticsDto
    {
        public int TotalSessions { get; set; }
        public int ActiveSessions { get; set; }
        public int CompletedSessions { get; set; }
        public decimal TotalRevenue { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public string MostPopularDevice { get; set; } = "";
    }
}
