using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Application.DTOs;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    /// <summary>
    /// Tenant-safe device service.
    /// The global EF query filter in ApplicationDbContext ensures every
    /// repository call only touches the current tenant's rows — no manual
    /// TenantId guard needed here.
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _repo;

        public DeviceService(IDeviceRepository repo)
        {
            _repo = repo;
        }

        // ══════════════════════════════════════════════════════════════════════
        // QUERIES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<DeviceDto>> GetAllAsync()
        {
            var devices = await _repo.GetAllActiveAsync();
            return devices.Select(Map);
        }

        public async Task<IEnumerable<DeviceDto>> GetAvailableAsync()
        {
            var devices = await _repo.GetAvailableAsync();
            return devices.Select(Map);
        }

        public async Task<IEnumerable<DeviceDto>> GetAllWithActiveSessionsAsync()
        {
            var devices = await _repo.GetAllWithActiveSessionsAsync();
            return devices.Select(MapWithSession);
        }

        public async Task<DeviceDto?> GetByIdAsync(int id)
        {
            // GetWithActiveSessionAsync pre-loads the Sessions navigation collection
            // so IsAvailable and CurrentSession are always accurate in memory.
            var device = await _repo.GetWithActiveSessionAsync(id);
            return device == null ? null : MapWithSession(device);
        }

        // ══════════════════════════════════════════════════════════════════════
        // COMMANDS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto)
        {
            var device = new Device
            {
                Name = dto.Name,
                Type = dto.Type,
                SingleSessionRate = dto.SingleSessionRate,
                MultiSessionRate = dto.MultiSessionRate,
                SupportsMultiSession = dto.SupportsMultiSession,
                IsActive = true,
                Status = DeviceStatus.Available
            };

            await _repo.AddAsync(device);
            return Map(device);
        }

        public async Task<DeviceDto> UpdateAsync(int id, UpdateDeviceDto dto)
        {
            var device = await _repo.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Device {id} not found.");

            device.Name = dto.Name;
            device.SingleSessionRate = dto.SingleSessionRate;
            device.MultiSessionRate = dto.MultiSessionRate;
            device.SupportsMultiSession = dto.SupportsMultiSession;
            device.IsActive = dto.IsActive;
            device.Status = dto.Status;

            await _repo.UpdateAsync(device);

            // Re-load with sessions so the returned DTO reflects the true state.
            var updated = await _repo.GetWithActiveSessionAsync(id);
            return updated == null ? Map(device) : MapWithSession(updated);
        }

        public async Task DeleteAsync(int id)
        {
            // DeviceRepository.DeleteAsync is a soft-delete (sets IsActive = false).
            var device = await _repo.GetWithActiveSessionAsync(id)
                ?? throw new InvalidOperationException($"Device {id} not found.");

            if (device.Sessions.Any(s => s.Status == SessionStatus.Active))
                throw new InvalidOperationException(
                    "Cannot delete a device that is currently in use.");

            await _repo.DeleteAsync(id);
        }

        public async Task UpdateStatusAsync(int id, DeviceStatus status)
        {
            var device = await _repo.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Device {id} not found.");

            device.Status = status;
            await _repo.UpdateAsync(device);
        }

        // ══════════════════════════════════════════════════════════════════════
        // MAPPING
        // ══════════════════════════════════════════════════════════════════════

        /// <summary>Basic map — no session data.</summary>
        private static DeviceDto Map(Device d) => new()
        {
            Id = d.Id,
            Name = d.Name,
            Type = d.Type,
            SingleSessionRate = d.SingleSessionRate,
            MultiSessionRate = d.MultiSessionRate,
            SupportsMultiSession = d.SupportsMultiSession,
            DeviceStatus = d.Status,
            IsActive = d.IsActive,
            ActiveSessionId = null,
            CurrentSession = null
        };

        /// <summary>
        /// Full map — reads the already-loaded Sessions navigation collection
        /// to attach an active session snapshot to the DeviceDto.
        /// </summary>
        private static DeviceDto MapWithSession(Device d)
        {
            var dto = Map(d);
            var active = d.Sessions.FirstOrDefault(s => s.Status == SessionStatus.Active);

            if (active != null)
            {
                dto.DeviceStatus = DeviceStatus.InUse;
                dto.ActiveSessionId = active.Id;
                dto.CurrentSession = MapToSessionDto(active, d.Name);
            }
            else
            {
                dto.DeviceStatus = DeviceStatus.Available;
                dto.ActiveSessionId = null;
                dto.CurrentSession = null;
            }

            return dto;
        }

        /// <summary>
        /// Maps Session entity → legacy SessionDto used by _DeviceCards.cshtml.
        /// SessionType is stored as "Single"/"Multi" in the DB; the view
        /// expects lowercase "single"/"multi".
        /// </summary>
        private static SessionDto MapToSessionDto(Session s, string deviceName) => new()
        {
            Id = s.Id,
            DeviceId = s.DeviceId ?? 0,
            DeviceName = deviceName,
            CustomerName = s.CustomerName,
            CustomerPhone = s.CustomerPhone,
            StartTime = s.StartTime,
            HourlyRate = s.HourlyRate,
            Status = s.Status.ToString(),
            Duration = s.Duration,
            TotalCost = Math.Round((decimal)s.Duration.TotalHours * s.HourlyRate, 2),
            SessionType = s.SessionType?.ToLower() ?? "single"
        };
    }
}
