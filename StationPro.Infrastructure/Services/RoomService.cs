using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Application.DTOs;
using StationPro.Domain.Entities;

namespace StationPro.Infrastructure.Services
{
    /// <summary>
    /// Tenant-safe room service.
    /// Reservations are stored as a lightweight in-process dictionary keyed by roomId.
    /// Swap with a repository when you add a Reservations DB table.
    /// </summary>
    public class RoomService : IRoomService
    {
        private readonly IRoomRepository _repo;

        private static readonly Dictionary<int, RoomReservationDto> _reservations = new();
        private static int _nextReservationId = 1;
        private static readonly object _resLock = new();

        public RoomService(IRoomRepository repo) => _repo = repo;

        // ── Queries ───────────────────────────────────────────────────────────

        public async Task<IEnumerable<RoomDto>> GetAllAsync()
        {
            var rooms = await _repo.GetAllActiveAsync();
            return rooms.Select(r => MapWithReservation(MapWithSession(r), r));
        }

        public async Task<IEnumerable<RoomDto>> GetAvailableAsync()
        {
            var rooms = await _repo.GetAvailableAsync();
            return rooms.Select(Map);
        }

        public async Task<IEnumerable<RoomDto>> GetAllWithActiveSessionsAsync()
        {
            var rooms = await _repo.GetAllWithActiveSessionsAsync();
            return rooms.Select(r => MapWithReservation(MapWithSession(r), r));
        }

        public async Task<RoomDto?> GetByIdAsync(int id)
        {
            var room = await _repo.GetWithActiveSessionAsync(id);
            if (room == null) return null;
            return MapWithReservation(MapWithSession(room), room);
        }

        // ── Commands ──────────────────────────────────────────────────────────

        public async Task<RoomDto> CreateAsync(RoomDto dto)
        {
            if (dto.SingleHourlyRate == 0 && dto.HourlyRate > 0)
                dto.SingleHourlyRate = dto.HourlyRate;
            if (dto.MultiHourlyRate == 0 && dto.SingleHourlyRate > 0)
                dto.MultiHourlyRate = dto.SingleHourlyRate;

            var room = new Room
            {
                Name = dto.Name,
                HasAC = dto.HasAC,
                SingleHourlyRate = dto.SingleHourlyRate,
                MultiHourlyRate = dto.MultiHourlyRate,
                Capacity = dto.Capacity,
                DeviceCount = dto.DeviceCount,
                IsActive = true
            };

            await _repo.AddAsync(room);
            return Map(room);
        }

        public async Task<RoomDto> UpdateAsync(int id, RoomDto dto)
        {
            var room = await _repo.GetByIdAsync(id)
                ?? throw new InvalidOperationException($"Room {id} not found.");

            room.Name = dto.Name;
            room.HasAC = dto.HasAC;
            room.SingleHourlyRate = dto.SingleHourlyRate;
            room.MultiHourlyRate = dto.MultiHourlyRate;
            room.Capacity = dto.Capacity;
            room.DeviceCount = dto.DeviceCount;
            room.IsActive = dto.IsActive;

            await _repo.UpdateAsync(room);

            var updated = await _repo.GetWithActiveSessionAsync(id);
            if (updated == null) return Map(room);
            return MapWithReservation(MapWithSession(updated), updated);
        }

        public async Task DeleteAsync(int id)
        {
            var room = await _repo.GetWithActiveSessionAsync(id)
                ?? throw new InvalidOperationException($"Room {id} not found.");

            if (room.Sessions.Any(s => s.Status == SessionStatus.Active))
                throw new InvalidOperationException(
                    "Cannot delete an occupied room. End the session first.");

            room.IsActive = false;
            await _repo.UpdateAsync(room);
            lock (_resLock) { _reservations.Remove(id); }
        }

        // ── Reservations ──────────────────────────────────────────────────────

        public Task<RoomReservationDto> AddReservationAsync(CreateReservationRequest request)
        {
            lock (_resLock)
            {
                var reservation = new RoomReservationDto
                {
                    Id = _nextReservationId++,
                    RoomId = request.RoomId,
                    ClientName = request.ClientName,
                    Phone = request.Phone ?? string.Empty,
                    ReservationTime = request.ReservationTime,
                    Notes = request.Notes ?? string.Empty
                };
                _reservations[request.RoomId] = reservation;
                return Task.FromResult(reservation);
            }
        }

        public Task<RoomReservationDto?> GetReservationAsync(int roomId)
        {
            lock (_resLock)
            {
                _reservations.TryGetValue(roomId, out var res);
                return Task.FromResult(res);
            }
        }

        public Task RemoveReservationAsync(int roomId)
        {
            lock (_resLock) { _reservations.Remove(roomId); }
            return Task.CompletedTask;
        }

        // ── Mapping ───────────────────────────────────────────────────────────

        private static RoomDto Map(Room r) => new()
        {
            Id = r.Id,
            Name = r.Name,
            HasAC = r.HasAC,
            SingleHourlyRate = r.SingleHourlyRate,
            MultiHourlyRate = r.MultiHourlyRate,
            Capacity = r.Capacity,
            DeviceCount = r.DeviceCount,
            IsActive = r.IsActive,
            Status = "Available",
            CurrentOccupancy = 0,
            HourlyRate = r.SingleHourlyRate,
            ActiveSessionId = null,
            SessionStartTime = null,
            SessionClientName = null,
            SessionType = null
        };

        private static RoomDto MapWithSession(Room r)
        {
            var dto = Map(r);
            var active = r.Sessions.FirstOrDefault(s => s.Status == SessionStatus.Active);

            if (active != null)
            {
                dto.Status = "Occupied";
                dto.CurrentOccupancy = active.GuestCount;
                dto.HourlyRate = active.HourlyRate;
                dto.ActiveSessionId = active.Id;
                dto.SessionClientName = active.CustomerName;
                dto.SessionType = active.SessionType;  // "Single" | "Multi"

                // ── UTC fix ────────────────────────────────────────────────────
                // EF Core returns datetime columns as DateTimeKind.Unspecified.
                // We tag it as Utc here so the Razor view can emit the correct
                // ISO string with a Z suffix, preventing the browser from
                // misinterpreting it as local time (which causes the 2-hour offset
                // bug visible in Egypt UTC+2 and any other non-UTC timezone).
                dto.SessionStartTime = DateTime.SpecifyKind(active.StartTime, DateTimeKind.Utc);
            }

            return dto;
        }

        private RoomDto MapWithReservation(RoomDto dto, Room r)
        {
            if (dto.Status != "Available") return dto;

            lock (_resLock)
            {
                if (!_reservations.TryGetValue(r.Id, out var res)) return dto;

                dto.Status = "Reserved";
                dto.ReservationClientName = res.ClientName;
                dto.ReservationTime = res.ReservationTime;
                dto.ReservationNotes = res.Notes;
            }

            return dto;
        }
    }
}