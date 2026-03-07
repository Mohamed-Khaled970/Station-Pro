using StationPro.Domain.Entities;

namespace StationPro.Application.DTOs
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DeviceType Type { get; set; }

        /// <summary>String version: "PS5", "PS4", "Xbox", "PC" etc.</summary>
        public string TypeName => Type.ToString();

        // Pricing
        public decimal SingleSessionRate { get; set; }
        public decimal? MultiSessionRate { get; set; }

        // THE FIX:
        // The previous version computed SupportsMultiSession as a read-only
        // property derived from Type. That broke DeviceController.Update() which
        // needs to write: device.SupportsMultiSession = updated.SupportsMultiSession
        //
        // Fix: keep it as a plain settable auto-property (admin can enable/disable
        // it independently of type), and add IsMultiSessionCapableByType as a
        // separate read-only helper for the type-based logic.
        // This mirrors exactly what the domain Device entity already does:
        // it has both a stored SupportsMultiSession field AND IsMultiSessionCapable().
        public bool SupportsMultiSession { get; set; }

        /// <summary>
        /// Whether the TYPE naturally supports multi-session (for UI hints).
        /// SupportsMultiSession is the actual admin-controlled flag.
        /// </summary>
        public bool IsMultiSessionCapableByType => Type switch
        {
            DeviceType.PS5 => true,
            DeviceType.PS4 => true,
            DeviceType.PS3 => true,
            DeviceType.Xbox => true,
            DeviceType.PingPong => true,
            DeviceType.Pool => true,
            DeviceType.Billiards => true,
            _ => false
        };

        /// <summary>Legacy alias so views/JS using HourlyRate still work.</summary>
        public decimal HourlyRate => SingleSessionRate;

        // Status - single source of truth is DeviceStatus enum
        public bool IsActive { get; set; } = true;
        public DeviceStatus DeviceStatus { get; set; } = DeviceStatus.Available;

        /// <summary>True only when active AND no session is running.</summary>
        public bool IsAvailable => IsActive && DeviceStatus == DeviceStatus.Available;

        /// <summary>String for views: "Available" | "InUse" | "Maintenance" | "Offline"</summary>
        public string Status => IsActive ? DeviceStatus.ToString() : "Offline";

        // Runtime session snapshot (never persisted to DB)
        public int? ActiveSessionId { get; set; }
        public SessionDto? CurrentSession { get; set; }

        // Display helpers
        public string RateDisplay => SupportsMultiSession && MultiSessionRate.HasValue
            ? $"Single: {SingleSessionRate:C} | Multi: {MultiSessionRate.Value:C}"
            : $"{SingleSessionRate:C}/hr";
    }
}