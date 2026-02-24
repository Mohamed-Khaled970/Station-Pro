using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class StartRoomSessionRequest
    {
        public int RoomId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public int GuestCount { get; set; } = 1;
    }
}
