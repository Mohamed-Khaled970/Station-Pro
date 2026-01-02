using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class CreateDeviceDto
    {
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }
        public decimal HourlyRate { get; set; }
        // Remove RoomId - not needed
    }
}
