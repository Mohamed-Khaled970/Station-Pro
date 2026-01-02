using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class UpdateDeviceDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
        public DeviceStatus Status { get; set; }
        // Remove RoomId - not needed
    }
}
