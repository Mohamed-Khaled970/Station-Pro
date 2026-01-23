using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class SessionDto
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal TotalCost { get; set; }
        public decimal HourlyRate { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationFormatted => $"{(int)Duration.TotalHours:00}:{Duration.Minutes:00}:{Duration.Seconds:00}";
        public string Status { get; set; } = "Active";
        // Add to both classes
        public string SessionType { get; set; } = "single"; // "single" or "multi"
        public string SessionTypeDisplay => SessionType == "multi" ? "Multi-Session" : "Single Session";
    }
}
