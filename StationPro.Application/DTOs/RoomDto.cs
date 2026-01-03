using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool HasAC { get; set; }
        public decimal HourlyRate { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "Available"; // Available, Occupied, Reserved, Maintenance
        public int CurrentOccupancy { get; set; }
        public int DeviceCount { get; set; }
    }
}
