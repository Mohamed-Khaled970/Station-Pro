using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    /// <summary>
    /// Summary statistics for a date range
    /// </summary>
    public class ReportSummaryDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalSessions { get; set; }
        public int CompletedSessions { get; set; }
        public int CancelledSessions { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageSessionRevenue { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public string MostUsedDevice { get; set; } = string.Empty;
        public string MostPopularTime { get; set; } = string.Empty; // e.g., "2 PM - 6 PM"
    }
}
