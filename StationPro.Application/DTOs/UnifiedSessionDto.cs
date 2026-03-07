using StationPro.Application.Enums;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.DTOs
{
    /// <summary>
    /// Single DTO that represents ANY session — device or room.
    /// Replaces both the old SessionDto and RoomSessionDto.
    /// </summary>
    public class UnifiedSessionDto
    {
        public int Id { get; set; }
        public int TenantId { get; set; }

        // ── Source — exactly one of these is set ──────────────────────────────
        public SessionSourceType SourceType { get; set; }
        public int? DeviceId { get; set; }
        public int? RoomId { get; set; }

        /// <summary>Human-readable name: device name OR room name.</summary>
        public string SourceName { get; set; } = string.Empty;

        /// <summary>Device type string (PS5, Xbox, PC …) or "Room".</summary>
        public string SourceCategory { get; set; } = string.Empty;

        // ── Session classification ─────────────────────────────────────────────
        public SessionType SessionType { get; set; } = SessionType.Single;

        // ── Customer ──────────────────────────────────────────────────────────
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }

        /// <summary>Number of guests — 1 for device single, up to 4 for room multi.</summary>
        public int GuestCount { get; set; } = 1;

        // ── Timing ────────────────────────────────────────────────────────────
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration => (EndTime ?? DateTime.UtcNow) - StartTime;
        public string DurationFormatted =>
            $"{(int)Duration.TotalHours:00}:{Duration.Minutes:00}:{Duration.Seconds:00}";

        // ── Pricing ───────────────────────────────────────────────────────────
        /// <summary>Rate snapshot taken at session start — never changes mid-session.</summary>
        public decimal HourlyRate { get; set; }
        public decimal TotalCost { get; set; }

        // ── State ─────────────────────────────────────────────────────────────
        public SessionStatus Status { get; set; } = SessionStatus.Active;
        public PaymentMethod? PaymentMethod { get; set; }

        // ── Computed helpers ──────────────────────────────────────────────────
        public bool IsActive => Status == SessionStatus.Active;
        public string SessionTypeDisplay => SessionType == SessionType.Multi
            ? "Multi-Session"
            : "Single Session";

        /// <summary>Live running cost for active sessions.</summary>
        public decimal RunningCost =>
            IsActive
                ? Math.Round((decimal)Duration.TotalHours * HourlyRate, 2)
                : TotalCost;

        // ── View-friendly string aliases ──────────────────────────────────────
        // These keep existing Razor views working without any view-side changes.

        /// <summary>Alias for views/JS that reference DeviceName.</summary>
        public string DeviceName => SourceName;

        /// <summary>Alias for views that reference DeviceType.</summary>
        public string DeviceType => SourceCategory;

        /// <summary>
        /// Lowercase string version of SessionType for views: "single" | "multi".
        /// Matches the string literals already used in the Session Index view.
        /// </summary>
        public string SessionTypeString => SessionType == SessionType.Multi ? "multi" : "single";

        /// <summary>
        /// String version of Status for views: "Active" | "Completed" | "Cancelled".
        /// Matches the string literals already used in the Session Index view.
        /// </summary>
        public string StatusString => Status.ToString();
    }
}