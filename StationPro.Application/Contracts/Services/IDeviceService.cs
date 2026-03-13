using StationPro.Application.DTOs;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Contracts.Services
{

    /// <summary>
    /// Device management — all operations are tenant-scoped by the
    /// global EF query filter registered in ApplicationDbContext.
    /// </summary>
    public interface IDeviceService
    {
        // ── Queries ───────────────────────────────────────────────────────────

        Task<IEnumerable<DeviceDto>> GetAllAsync();
        Task<IEnumerable<DeviceDto>> GetAvailableAsync();
        Task<IEnumerable<DeviceDto>> GetAllWithActiveSessionsAsync();
        Task<DeviceDto?> GetByIdAsync(int id);

        // ── Commands ──────────────────────────────────────────────────────────

        Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
        Task<DeviceDto> UpdateAsync(int id, UpdateDeviceDto dto);

        /// <summary>Soft-delete — sets IsActive = false.</summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Persists a status change — called internally by SessionService
        /// after starting / ending a session.
        /// </summary>
        Task UpdateStatusAsync(int id, DeviceStatus status);
    }
}
