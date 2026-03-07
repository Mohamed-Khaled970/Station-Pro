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

        // ── Pricing (replaces the single legacy HourlyRate) ───────────────────
        public decimal SingleHourlyRate { get; set; }
        public decimal MultiHourlyRate { get; set; }

        public int Capacity { get; set; } = 1;
        public int DeviceCount { get; set; }
        public bool IsActive { get; set; } = true;

        // ── Status ────────────────────────────────────────────────────────────
        /// <summary>Available | Occupied | Reserved | Maintenance</summary>
        public string Status { get; set; } = "Available";

        // ── Active session snapshot (populated when Occupied) ─────────────────
        public int? ActiveSessionId { get; set; }
        public DateTime? SessionStartTime { get; set; }
        public string? SessionClientName { get; set; }
        public string? SessionType { get; set; }        // "Single" | "Multi"
        public decimal HourlyRate { get; set; }         // Live rate for card display
        public int CurrentOccupancy { get; set; }

        // ── Reservation snapshot (populated when Reserved) ────────────────────
        public string? ReservationClientName { get; set; }
        public DateTime? ReservationTime { get; set; }
        public string? ReservationNotes { get; set; }
    }
}