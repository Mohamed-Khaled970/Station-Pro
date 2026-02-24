using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    public class CreateReservationRequest
    {
        public int RoomId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime ReservationTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
