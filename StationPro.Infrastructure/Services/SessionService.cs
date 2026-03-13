using StationPro.Application.Contracts.Repositories;
using StationPro.Application.Contracts.Services;
using StationPro.Application.DTOs;
using StationPro.Application.Enums;
using StationPro.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Infrastructure.Services
{
    /// <summary>
    /// DB-backed unified session service.
    /// Handles both device and room sessions through a single UnifiedSessionDto.
    /// Tenant scoping is enforced entirely by the global EF query filter —
    /// no manual TenantId checks needed anywhere in this class.
    /// </summary>
    public class SessionService : ISessionService
    {
        private readonly ISessionRepository _sessions;
        private readonly IDeviceRepository _devices;
        private readonly IRoomRepository _rooms;

        public SessionService(
            ISessionRepository sessions,
            IDeviceRepository devices,
            IRoomRepository rooms)
        {
            _sessions = sessions;
            _devices = devices;
            _rooms = rooms;
        }

        // ══════════════════════════════════════════════════════════════════════
        // QUERIES
        // ══════════════════════════════════════════════════════════════════════

        public async Task<IEnumerable<UnifiedSessionDto>> GetActiveAsync()
        {
            var active = await _sessions.GetActiveAsync();
            return active.Select(Map);
        }

        public async Task<IEnumerable<UnifiedSessionDto>> GetActiveForDevicesAsync()
        {
            var active = await _sessions.GetActiveAsync();
            return active
                .Where(s => s.SourceType == "Device")
                .Select(Map);
        }

        public async Task<IEnumerable<UnifiedSessionDto>> GetActiveForRoomsAsync()
        {
            var active = await _sessions.GetActiveAsync();
            return active
                .Where(s => s.SourceType == "Room")
                .Select(Map);
        }

        public async Task<UnifiedSessionDto?> GetByIdAsync(int id)
        {
            var session = await _sessions.GetByIdAsync(id);
            return session == null ? null : Map(session);
        }

        public async Task<SessionPageResult> GetPageAsync(SessionFilterRequest filter)
        {
            var (items, total) = await _sessions.GetPagedAsync(filter);

            var sessions = items.Select(Map).ToList();
            var totalPages = (int)Math.Ceiling(total / (double)filter.PageSize);

            return new SessionPageResult
            {
                Sessions = sessions,
                TotalCount = total,
                CurrentPage = filter.Page,
                TotalPages = totalPages,
                Statistics = BuildStatistics(sessions)
            };
        }

        public async Task<decimal> GetDailyRevenueAsync(DateTime date)
            => await _sessions.GetDailyRevenueAsync(date);

        // ── Dashboard stats ────────────────────────────────────────────────────

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var revenue = await _sessions.GetDailyRevenueAsync(today);

            var allActive = (await _sessions.GetActiveAsync()).ToList();
            var allToday = (await _sessions.GetAllAsync(from: today)).ToList();
            var totalDevs = (await _devices.GetAllActiveAsync()).Count();

            // Average running cost across currently active sessions
            var avgCost = allActive.Any()
                ? allActive.Average(s =>
                    Math.Round((decimal)(DateTime.UtcNow - s.StartTime).TotalHours * s.HourlyRate, 2))
                : 0m;

            return new DashboardStatsDto
            {
                TodayRevenue = revenue,
                TotalSessions = allToday.Count,
                ActiveSessions = allActive.Count,
                ActiveDevices = allActive.Count(s => s.SourceType == "Device"),
                TotalDevices = totalDevs,
                AverageSessionCost = Math.Round(avgCost, 2)
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // DEVICE SESSIONS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<StartSessionResponse> StartDeviceSessionAsync(
            StartDeviceSessionRequest request,
            int deviceId)
        {
            // Load device with its active sessions so IsAvailable is accurate
            var device = await _devices.GetWithActiveSessionAsync(deviceId)
                ?? throw new InvalidOperationException("Device not found.");

            if (device.Status != DeviceStatus.Available)
                throw new InvalidOperationException($"Device is currently {device.Status}.");

            var isMulti = request.SessionType?.ToLower() == "multi"
                          && device.SupportsMultiSession
                          && device.MultiSessionRate.HasValue;

            var rate = isMulti ? device.MultiSessionRate!.Value : device.SingleSessionRate;
            var sessionType = isMulti ? "Multi" : "Single";

            var session = new Session
            {
                SourceType = "Device",
                DeviceId = deviceId,
                SessionType = sessionType,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                GuestCount = 1,
                StartTime = DateTime.UtcNow,
                HourlyRate = rate,
                Status = SessionStatus.Active
            };

            await _sessions.AddAsync(session);
            session.StartTime = DateTime.SpecifyKind(session.StartTime, DateTimeKind.Utc);
            // Mark device as in-use
            device.Status = DeviceStatus.InUse;
            await _devices.UpdateAsync(device);

            return new StartSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = request.CustomerName ?? string.Empty,
                GuestCount = 1,
                SessionType = sessionType,
                HourlyRate = rate,
                StartTime = session.StartTime
            };
        }

        public async Task<EndSessionResponse> EndDeviceSessionAsync(
            int sessionId,
            int paymentMethod)
        {
            var session = await _sessions.GetByIdAsync(sessionId)
                ?? throw new InvalidOperationException("Session not found.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException("Session is not active.");

            if (!session.DeviceId.HasValue)
                throw new InvalidOperationException("Session is not linked to a device.");

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Completed;
            session.TotalCost = Math.Round(
                (decimal)session.Duration.TotalHours * session.HourlyRate, 2);
            session.PaymentMethod = paymentMethod == 2 ? "Card" : "Cash";

            await _sessions.UpdateAsync(session);

            // Free the device
            var device = await _devices.GetByIdAsync(session.DeviceId.Value);
            if (device != null)
            {
                device.Status = DeviceStatus.Available;
                await _devices.UpdateAsync(device);
            }

            return new EndSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = session.CustomerName ?? string.Empty,
                SessionType = session.SessionType,
                Duration = FormatDuration(session.Duration),
                TotalCost = session.TotalCost,
                HourlyRate = session.HourlyRate
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // ROOM SESSIONS
        // ══════════════════════════════════════════════════════════════════════

        public async Task<StartSessionResponse> StartRoomSessionAsync(
            StartRoomSessionRequest request,
            int roomId)
        {
            var room = await _rooms.GetWithActiveSessionAsync(roomId)
                ?? throw new InvalidOperationException("Room not found.");

            if (!room.IsAvailable)
                throw new InvalidOperationException("Room is currently occupied.");

            var isMulti = request.SessionType?.Equals("Multi", StringComparison.OrdinalIgnoreCase) == true;
            int maxGuests = isMulti ? 4 : 2;

            if (request.GuestCount < 1 || request.GuestCount > maxGuests)
                throw new ArgumentException(
                    $"Guest count for a {request.SessionType} session must be between 1 and {maxGuests}.");

            var rate = isMulti ? room.MultiHourlyRate : room.SingleHourlyRate;
            var sessionType = isMulti ? "Multi" : "Single";

            var session = new Session
            {
                SourceType = "Room",
                RoomId = roomId,
                SessionType = sessionType,
                CustomerName = request.ClientName,
                GuestCount = request.GuestCount,
                StartTime = DateTime.UtcNow,
                HourlyRate = rate,
                Status = SessionStatus.Active
            };

            await _sessions.AddAsync(session);

            session.StartTime = DateTime.SpecifyKind(session.StartTime, DateTimeKind.Utc);
            return new StartSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = request.ClientName,
                GuestCount = request.GuestCount,
                SessionType = sessionType,
                HourlyRate = rate,
                StartTime = session.StartTime
            };
        }

        public async Task<EndSessionResponse> EndRoomSessionAsync(int sessionId)
        {
            var session = await _sessions.GetByIdAsync(sessionId)
                ?? throw new InvalidOperationException("Session not found.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException("Session is not active.");

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Completed;
            session.TotalCost = Math.Round(
                (decimal)session.Duration.TotalHours * session.HourlyRate, 2);
            session.PaymentMethod = "Cash";

            await _sessions.UpdateAsync(session);

            return new EndSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = session.CustomerName ?? string.Empty,
                SessionType = session.SessionType,
                Duration = FormatDuration(session.Duration),
                TotalCost = session.TotalCost,
                HourlyRate = session.HourlyRate
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // RECEIPT
        // ══════════════════════════════════════════════════════════════════════

        public async Task<SessionReceiptDto?> GetReceiptAsync(int sessionId)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null) return null;

            string sourceName = session.Device?.Name ?? session.Room?.Name ?? string.Empty;
            string sourceCategory = session.Device != null
                ? session.Device.Type.ToString()
                : "Room";

            return new SessionReceiptDto
            {
                SessionId = session.Id,
                SourceName = sourceName,
                SourceCategory = sourceCategory,
                CustomerName = session.CustomerName,
                SessionType = session.SessionType,
                GuestCount = session.GuestCount,
                StartTime = session.StartTime,
                EndTime = session.EndTime ?? DateTime.UtcNow,
                Duration = session.Duration,
                DurationFormatted = FormatDuration(session.Duration),
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                PaymentMethod = session.PaymentMethod ?? "Cash",
                CompletedAt = session.EndTime ?? DateTime.UtcNow
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // MAPPING  — Session entity → UnifiedSessionDto
        // ══════════════════════════════════════════════════════════════════════

        private static UnifiedSessionDto Map(Session s)
        {
            // Resolve source type enum
            var sourceType = s.SourceType == "Room"
                ? SessionSourceType.Room
                : SessionSourceType.Device;

            // Resolve session type enum
            var sessionType = s.SessionType?.Equals("Multi", StringComparison.OrdinalIgnoreCase) == true
                ? SessionType.Multi
                : SessionType.Single;

            // Resolve payment method enum
            PaymentMethod? payment = s.PaymentMethod switch
            {
                "Card" => PaymentMethod.Card,
                "Cash" => PaymentMethod.Cash,
                _ => null
            };

            // Running cost for active sessions; stored cost for completed ones
            var elapsed = (s.EndTime ?? DateTime.UtcNow) - s.StartTime;
            var runningCost = s.Status == SessionStatus.Active
                ? Math.Round((decimal)elapsed.TotalHours * s.HourlyRate, 2)
                : s.TotalCost;

            return new UnifiedSessionDto
            {
                Id = s.Id,
                TenantId = s.TenantId,
                SourceType = sourceType,
                DeviceId = s.DeviceId,
                RoomId = s.RoomId,
                SourceName = s.Device?.Name ?? s.Room?.Name ?? string.Empty,
                SourceCategory = s.Device != null ? s.Device.Type.ToString() : "Room",
                SessionType = sessionType,
                CustomerName = s.CustomerName,
                CustomerPhone = s.CustomerPhone,
                GuestCount = s.GuestCount,
                StartTime = DateTime.SpecifyKind(s.StartTime, DateTimeKind.Utc),
                EndTime = s.EndTime.HasValue
             ? DateTime.SpecifyKind(s.EndTime.Value, DateTimeKind.Utc)
             : (DateTime?)null,
                HourlyRate = s.HourlyRate,
                TotalCost = runningCost,
                Status = s.Status,
                PaymentMethod = payment
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        // STATISTICS  — built from UnifiedSessionDto list
        // ══════════════════════════════════════════════════════════════════════

        private static SessionStatisticsDto BuildStatistics(IEnumerable<UnifiedSessionDto> sessions)
        {
            var list = sessions.ToList();
            var totalRevenue = list.Sum(s => s.RunningCost);

            return new SessionStatisticsDto
            {
                TotalSessions = list.Count,
                ActiveSessions = list.Count(s => s.IsActive),
                CompletedSessions = list.Count(s => s.Status == SessionStatus.Completed),
                TotalRevenue = totalRevenue,
                AverageDuration = list.Any()
                    ? TimeSpan.FromMinutes(list.Average(s => s.Duration.TotalMinutes))
                    : TimeSpan.Zero,
                MostPopularDevice = list
                    .GroupBy(s => s.SourceName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };
        }

        private static string FormatDuration(TimeSpan d)
            => $"{(int)d.TotalHours:D2}:{d.Minutes:D2}:{d.Seconds:D2}";
    }
}
