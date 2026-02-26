using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class RoomSessionDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int GuestCount { get; set; }

        /// <summary>"Single" (≤ 2 persons) or "Multi" (≤ 4 persons).</summary>
        public string SessionType { get; set; } = "Single";

        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal HourlyRate { get; set; }   // snapshot of the rate used at start
        public decimal TotalCost { get; set; }
        public bool IsActive { get; set; } = true;
    }
}