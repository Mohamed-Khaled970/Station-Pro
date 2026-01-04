using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    /// <summary>
    /// Complete report with all data
    /// </summary>
    public class CompleteReportDto
    {
        public ReportSummaryDto Summary { get; set; } = new();
        public List<SessionReportDto> Sessions { get; set; } = new();
        public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
        public List<DevicePerformanceDto> DevicePerformance { get; set; } = new();
        public List<HourlyUsageDto> HourlyUsage { get; set; } = new();
    }
}
