using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    // DTO for Session Receipt
    public class SessionReceiptDto
    {
        public int SessionId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationFormatted { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public decimal TotalCost { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }
}
