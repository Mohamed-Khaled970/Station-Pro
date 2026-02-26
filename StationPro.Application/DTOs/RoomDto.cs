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

        // ── Dual pricing ──────────────────────────────────────────────────────
        /// <summary>Rate applied when session type is Single (≤ 2 persons).</summary>
        public decimal SingleHourlyRate { get; set; }

        /// <summary>Rate applied when session type is Multi (≤ 4 persons).</summary>
        public decimal MultiHourlyRate { get; set; }

        // Legacy property — kept so existing receipt / dashboard code that reads
        // HourlyRate still compiles. It is populated by the controller from the
        // active session's chosen rate and should NOT be stored on the room itself.
        public decimal HourlyRate { get; set; }

        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = "Available"; // Available, Occupied, Reserved, Maintenance
        public int CurrentOccupancy { get; set; }
        public int DeviceCount { get; set; }

        // Session info (populated when Occupied)
        public DateTime? SessionStartTime { get; set; }
        public string? SessionClientName { get; set; }
        public int? ActiveSessionId { get; set; }
        public string? SessionType { get; set; }   // "Single" | "Multi"

        // Reservation info (populated when Reserved)
        public string? ReservationClientName { get; set; }
        public DateTime? ReservationTime { get; set; }
        public string? ReservationNotes { get; set; }
    }
}