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

        // Updated to use single and multi session rates
        public decimal SingleSessionRate { get; set; }
        public decimal? MultiSessionRate { get; set; }
        public bool SupportsMultiSession { get; set; }

        public bool IsActive { get; set; }
        public DeviceStatus Status { get; set; }
    }
}
