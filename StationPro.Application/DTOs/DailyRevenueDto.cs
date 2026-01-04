using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{

    /// <summary>
    /// Daily revenue breakdown
    /// </summary>
    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public string DateFormatted { get; set; } = string.Empty;
        public int SessionCount { get; set; }
        public decimal Revenue { get; set; }
        public string DayOfWeek { get; set; } = string.Empty;
    }
}
