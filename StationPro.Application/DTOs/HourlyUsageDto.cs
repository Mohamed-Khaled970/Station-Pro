using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    /// <summary>
    /// Hourly usage pattern
    /// </summary>
    public class HourlyUsageDto
    {
        public int Hour { get; set; }
        public string TimeRange { get; set; } = string.Empty; // "2 PM - 3 PM"
        public int SessionCount { get; set; }
        public decimal Revenue { get; set; }
    }
}
