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
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalCost { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
