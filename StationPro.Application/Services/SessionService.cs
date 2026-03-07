using StationPro.Application.DTOs;
using StationPro.Application.Enums;
using StationPro.Application.Interfaces;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Domain.Entities;

namespace StationPro.Application.Services
{
    public class SessionService : ISessionService
    {
        // ═══════════════════════════════════════════════════════════════════════
        // QUERIES
        // ═══════════════════════════════════════════════════════════════════════

        public List<UnifiedSessionDto> GetActiveSessions()
            => SessionStore.GetActive();

        public UnifiedSessionDto? GetById(int sessionId)
            => SessionStore.GetById(sessionId);

        public SessionPageResult GetPage(SessionFilterRequest filter)
        {
            // FIX: No longer calling GenerateDummyHistoricalSessions() here.
            // Previously the dummy sessions were generated fresh on every call
            // and NEVER stored in SessionStore, so GetById() always returned null
            // for any of them. All dummy data is now seeded in SessionStore.Seed()
            // at startup, so GetAll() already includes them and they are fully
            // retrievable by ID.
            var all = SessionStore.GetAll();

            var filtered = ApplyFilters(all, filter);
            var total = filtered.Count;
            var totalPages = (int)Math.Ceiling(total / (double)filter.PageSize);

            var page = filtered
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new SessionPageResult
            {
                Sessions = page,
                TotalCount = total,
                CurrentPage = filter.Page,
                TotalPages = totalPages,
                Statistics = BuildStatistics(filtered)
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // DEVICE SESSIONS
        // ═══════════════════════════════════════════════════════════════════════

        public StartSessionResponse StartDeviceSession(
            StartDeviceSessionRequest request,
            DeviceDto device)
        {
            // IsAvailable is now computed (IsActive && DeviceStatus == Available)
            // so this check is always accurate.
            if (!device.IsAvailable)
                throw new InvalidOperationException($"Device is currently {device.Status}.");

            var isMulti = request.SessionType?.ToLower() == "multi"
                          && device.SupportsMultiSession
                          && device.MultiSessionRate.HasValue;

            var rate = isMulti ? device.MultiSessionRate!.Value : device.SingleSessionRate;
            var sessionType = isMulti ? SessionType.Multi : SessionType.Single;

            var session = SessionStore.Add(new UnifiedSessionDto
            {
                TenantId = 0,
                SourceType = SessionSourceType.Device,
                DeviceId = device.Id,
                SourceName = device.Name,
                SourceCategory = device.TypeName,
                SessionType = sessionType,
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                GuestCount = 1,
                StartTime = DateTime.UtcNow,
                HourlyRate = rate,
                Status = SessionStatus.Active
            });

            device.DeviceStatus = DeviceStatus.InUse;
            device.ActiveSessionId = session.Id;
            device.CurrentSession = MapToSessionDto(session);
            // caller (controller) calls DeviceStore.Update(device) after this

            return new StartSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = request.CustomerName ?? string.Empty,
                GuestCount = 1,
                SessionType = sessionType.ToString(),
                HourlyRate = rate,
                StartTime = session.StartTime
            };
        }

        public EndSessionResponse EndDeviceSession(
            int sessionId,
            int paymentMethod,
            DeviceDto device)
        {
            var session = SessionStore.GetById(sessionId)
                ?? throw new InvalidOperationException("Session not found.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException("Session is not active.");

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Completed;
            session.TotalCost = Math.Round(
                (decimal)session.Duration.TotalHours * session.HourlyRate, 2);
            session.PaymentMethod = paymentMethod == 2
                ? PaymentMethod.Card
                : PaymentMethod.Cash;

            SessionStore.Update(session);

            device.DeviceStatus = DeviceStatus.Available;
            device.ActiveSessionId = null;
            device.CurrentSession = null;
            // caller calls DeviceStore.Update(device) after this

            return new EndSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = session.CustomerName ?? string.Empty,
                SessionType = session.SessionType.ToString(),
                Duration = session.DurationFormatted,
                TotalCost = session.TotalCost,
                HourlyRate = session.HourlyRate
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ROOM SESSIONS
        // ═══════════════════════════════════════════════════════════════════════

        public StartSessionResponse StartRoomSession(
            StartRoomSessionRequest request,
            RoomDto room)
        {
            if (room.Status != "Available")
                throw new InvalidOperationException($"Room is currently {room.Status}.");

            var isMulti = request.SessionType?.Equals("Multi", StringComparison.OrdinalIgnoreCase) == true;
            int maxGuests = isMulti ? 4 : 2;

            if (request.GuestCount < 1 || request.GuestCount > maxGuests)
                throw new ArgumentException(
                    $"Guest count for a {request.SessionType} session must be between 1 and {maxGuests}.");

            var rate = isMulti ? room.MultiHourlyRate : room.SingleHourlyRate;
            var sessionType = isMulti ? SessionType.Multi : SessionType.Single;

            var session = SessionStore.Add(new UnifiedSessionDto
            {
                TenantId = 0,
                SourceType = SessionSourceType.Room,
                RoomId = room.Id,
                SourceName = room.Name,
                SourceCategory = "Room",
                SessionType = sessionType,
                CustomerName = request.ClientName,
                GuestCount = request.GuestCount,
                StartTime = DateTime.UtcNow,
                HourlyRate = rate,
                Status = SessionStatus.Active
            });

            room.Status = "Occupied";
            room.CurrentOccupancy = request.GuestCount;
            room.SessionStartTime = session.StartTime;
            room.SessionClientName = request.ClientName;
            room.ActiveSessionId = session.Id;
            room.SessionType = sessionType.ToString();
            room.HourlyRate = rate;
            // caller calls RoomStore.Update(room)

            return new StartSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = request.ClientName,
                GuestCount = request.GuestCount,
                SessionType = sessionType.ToString(),
                HourlyRate = rate,
                StartTime = session.StartTime
            };
        }

        public EndSessionResponse EndRoomSession(int sessionId, RoomDto room)
        {
            var session = SessionStore.GetById(sessionId)
                ?? throw new InvalidOperationException("Session not found.");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException("Session is not active.");

            session.EndTime = DateTime.UtcNow;
            session.Status = SessionStatus.Completed;
            session.TotalCost = Math.Round(
                (decimal)session.Duration.TotalHours * session.HourlyRate, 2);
            session.PaymentMethod = PaymentMethod.Cash;

            SessionStore.Update(session);

            room.Status = "Available";
            room.CurrentOccupancy = 0;
            room.SessionStartTime = null;
            room.SessionClientName = null;
            room.ActiveSessionId = null;
            room.SessionType = null;
            room.HourlyRate = 0;
            // caller calls RoomStore.Update(room)

            return new EndSessionResponse
            {
                Success = true,
                SessionId = session.Id,
                ClientName = session.CustomerName ?? string.Empty,
                SessionType = session.SessionType.ToString(),
                Duration = session.DurationFormatted,
                TotalCost = session.TotalCost,
                HourlyRate = session.HourlyRate
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // RECEIPT
        // ═══════════════════════════════════════════════════════════════════════

        public SessionReceiptDto? GetReceipt(int sessionId)
        {
            var session = SessionStore.GetById(sessionId);
            if (session == null) return null;

            return new SessionReceiptDto
            {
                SessionId = session.Id,
                SourceName = session.SourceName,
                SourceCategory = session.SourceCategory,
                CustomerName = session.CustomerName,
                SessionType = session.SessionType.ToString(),
                GuestCount = session.GuestCount,
                StartTime = session.StartTime,
                EndTime = session.EndTime ?? DateTime.UtcNow,
                Duration = session.Duration,
                DurationFormatted = session.DurationFormatted,
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                PaymentMethod = session.PaymentMethod?.ToString() ?? "Cash",
                CompletedAt = session.EndTime ?? DateTime.UtcNow
            };
        }

        // ═══════════════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ═══════════════════════════════════════════════════════════════════════

        private static List<UnifiedSessionDto> ApplyFilters(
            List<UnifiedSessionDto> sessions,
            SessionFilterRequest f)
        {
            var q = sessions.AsQueryable();

            q = f.DateFilter switch
            {
                "today" => q.Where(s => s.StartTime.Date == DateTime.Today),
                "yesterday" => q.Where(s => s.StartTime.Date == DateTime.Today.AddDays(-1)),
                "week" => q.Where(s => s.StartTime >= DateTime.Today.AddDays(-7)),
                "month" => q.Where(s => s.StartTime >= DateTime.Today.AddMonths(-1)),
                _ => q
            };

            if (f.Status != "all")
            {
                var targetStatus = f.Status.ToLower() == "active"
                    ? SessionStatus.Active
                    : SessionStatus.Completed;
                q = q.Where(s => s.Status == targetStatus);
            }

            if (f.DeviceId.HasValue)
                q = q.Where(s => s.DeviceId == f.DeviceId);

            if (!string.IsNullOrWhiteSpace(f.Search))
                q = q.Where(s =>
                    (s.CustomerName != null &&
                     s.CustomerName.Contains(f.Search, StringComparison.OrdinalIgnoreCase)) ||
                    s.SourceName.Contains(f.Search, StringComparison.OrdinalIgnoreCase));

            return q.OrderByDescending(s => s.StartTime).ToList();
        }

        private static SessionStatisticsDto BuildStatistics(List<UnifiedSessionDto> sessions)
        {
            var totalRevenue = sessions.Sum(s =>
                s.Status == SessionStatus.Active
                    ? Math.Round((decimal)s.Duration.TotalHours * s.HourlyRate, 2)
                    : s.TotalCost);

            return new SessionStatisticsDto
            {
                TotalSessions = sessions.Count,
                ActiveSessions = sessions.Count(s => s.Status == SessionStatus.Active),
                CompletedSessions = sessions.Count(s => s.Status == SessionStatus.Completed),
                TotalRevenue = totalRevenue,
                AverageDuration = sessions.Any()
                    ? TimeSpan.FromMinutes(sessions.Average(s => s.Duration.TotalMinutes))
                    : TimeSpan.Zero,
                MostPopularDevice = sessions
                    .GroupBy(s => s.SourceName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };
        }

        // ── Map UnifiedSessionDto → legacy SessionDto for device card display ──

        private static SessionDto MapToSessionDto(UnifiedSessionDto s) => new()
        {
            Id = s.Id,
            DeviceId = s.DeviceId ?? 0,
            DeviceName = s.SourceName,
            CustomerName = s.CustomerName,
            CustomerPhone = s.CustomerPhone,
            StartTime = s.StartTime,
            HourlyRate = s.HourlyRate,
            Status = s.Status.ToString(),
            Duration = s.Duration,
            TotalCost = s.TotalCost,
            SessionType = s.SessionType.ToString().ToLower()  // legacy: "single" | "multi"
        };
    }
}