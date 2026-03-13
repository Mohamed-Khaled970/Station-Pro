using StationPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{
    /// <summary>
    /// Unified session service — handles both device and room sessions.
    /// All results use UnifiedSessionDto as the canonical DTO.
    /// Tenant scoping is enforced by the global EF query filter.
    /// </summary>
    public interface ISessionService
    {
        // ── Queries ───────────────────────────────────────────────────────────

        /// <summary>All active sessions (devices + rooms).</summary>
        Task<IEnumerable<UnifiedSessionDto>> GetActiveAsync();

        /// <summary>Active device-only sessions — for the dashboard active-sessions panel.</summary>
        Task<IEnumerable<UnifiedSessionDto>> GetActiveForDevicesAsync();

        /// <summary>Active room-only sessions — for the rooms panel.</summary>
        Task<IEnumerable<UnifiedSessionDto>> GetActiveForRoomsAsync();

        /// <summary>Single session by id — null if not found.</summary>
        Task<UnifiedSessionDto?> GetByIdAsync(int id);

        /// <summary>Paged + filtered result set.</summary>
        Task<SessionPageResult> GetPageAsync(SessionFilterRequest filter);

        /// <summary>Total revenue for a given calendar day (completed + running).</summary>
        Task<decimal> GetDailyRevenueAsync(DateTime date);

        /// <summary>All stats needed to render the dashboard stats strip.</summary>
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        // ── Device sessions ───────────────────────────────────────────────────

        Task<StartSessionResponse> StartDeviceSessionAsync(StartDeviceSessionRequest request, int deviceId);
        Task<EndSessionResponse> EndDeviceSessionAsync(int sessionId, int paymentMethod);

        // ── Room sessions ─────────────────────────────────────────────────────

        Task<StartSessionResponse> StartRoomSessionAsync(StartRoomSessionRequest request, int roomId);
        Task<EndSessionResponse> EndRoomSessionAsync(int sessionId);

        // ── Receipt ───────────────────────────────────────────────────────────

        Task<SessionReceiptDto?> GetReceiptAsync(int sessionId);
    }
}
