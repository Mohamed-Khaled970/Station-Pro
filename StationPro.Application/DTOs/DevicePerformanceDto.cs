using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    /// <summary>
    /// Device performance statistics
    /// </summary>
    public class DevicePerformanceDto
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public TimeSpan TotalUsageTime { get; set; }
        public decimal TotalRevenue { get; set; }
        public double UtilizationPercentage { get; set; }
        public decimal AverageSessionCost { get; set; }
    }
}
