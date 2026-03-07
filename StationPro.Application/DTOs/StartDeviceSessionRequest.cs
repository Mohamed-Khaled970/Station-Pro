using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class StartDeviceSessionRequest
    {
        public int DeviceId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        /// <summary>"single" | "multi"</summary>
        public string SessionType { get; set; } = "single";
    }
}
