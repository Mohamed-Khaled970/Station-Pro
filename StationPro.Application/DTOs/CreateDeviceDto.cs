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

        // Single session rate (required)
        public decimal SingleSessionRate { get; set; }

        // Multi session rate (optional)
        public decimal? MultiSessionRate { get; set; }

        // Whether multi-session is enabled
        public bool SupportsMultiSession { get; set; } = false;
    }
}
