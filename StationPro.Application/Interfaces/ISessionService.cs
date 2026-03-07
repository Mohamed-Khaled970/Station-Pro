using StationPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Interfaces
{

    /// <summary>
    /// Single service contract for ALL session operations — device and room.
    /// Controllers call this; the implementation holds the in-memory store.
    /// </summary>
    public interface ISessionService
    {
        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>All active sessions (both device and room).</summary>
        List<UnifiedSessionDto> GetActiveSessions();

        /// <summary>Single session by ID. Returns null if not found.</summary>
        UnifiedSessionDto? GetById(int sessionId);

        /// <summary>Filtered, paginated list for the Sessions page.</summary>
        SessionPageResult GetPage(SessionFilterRequest filter);

        // ── Device sessions ───────────────────────────────────────────────────

        StartSessionResponse StartDeviceSession(StartDeviceSessionRequest request, DeviceDto device);

        EndSessionResponse EndDeviceSession(int sessionId, int paymentMethod, DeviceDto device);

        // ── Room sessions ─────────────────────────────────────────────────────

        StartSessionResponse StartRoomSession(StartRoomSessionRequest request, RoomDto room);

        EndSessionResponse EndRoomSession(int sessionId, RoomDto room);

        // ── Receipts ──────────────────────────────────────────────────────────

        SessionReceiptDto? GetReceipt(int sessionId);
    }
}
