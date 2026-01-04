using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    /// <summary>
    /// Detailed session information for reports
    /// </summary>
    public class SessionReportDto
    {
        public int Id { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationFormatted { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = string.Empty; // Completed, Cancelled
        public string PaymentMethod { get; set; } = string.Empty;
    }

}
