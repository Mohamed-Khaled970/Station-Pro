using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class StartSessionResponse
    {
        public bool Success { get; set; }
        public int SessionId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int GuestCount { get; set; }
        public string SessionType { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public DateTime StartTime { get; set; }
    }
}
