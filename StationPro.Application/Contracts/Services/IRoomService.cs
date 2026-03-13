using StationPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{
    /// <summary>
    /// Room management — tenant-scoped by the global EF query filter.
    /// </summary>
    public interface IRoomService
    {
        // ── Queries ───────────────────────────────────────────────────────────

        Task<IEnumerable<RoomDto>> GetAllAsync();
        Task<IEnumerable<RoomDto>> GetAvailableAsync();
        Task<IEnumerable<RoomDto>> GetAllWithActiveSessionsAsync();
        Task<RoomDto?> GetByIdAsync(int id);

        // ── Commands ──────────────────────────────────────────────────────────

        Task<RoomDto> CreateAsync(RoomDto dto);
        Task<RoomDto> UpdateAsync(int id, RoomDto dto);
        Task DeleteAsync(int id);

        // ── Reservations ──────────────────────────────────────────────────────

        Task<RoomReservationDto> AddReservationAsync(CreateReservationRequest request);
        Task<RoomReservationDto?> GetReservationAsync(int roomId);
        Task RemoveReservationAsync(int roomId);
    }
}
