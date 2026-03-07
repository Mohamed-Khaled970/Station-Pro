using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class EndSessionResponse
    {
        public bool Success { get; set; }
        public int SessionId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string SessionType { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public decimal HourlyRate { get; set; }
    }
}
