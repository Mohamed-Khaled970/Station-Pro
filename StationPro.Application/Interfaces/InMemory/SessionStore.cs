using StationPro.Application.DTOs;
using StationPro.Application.Enums;
using StationPro.Domain.Entities;

namespace StationPro.Application.Interfaces.InMemory
{
    /// <summary>
    /// Central in-memory store for ALL sessions.
    /// This is the ONLY place session data lives.
    /// Replace this class body with EF Core calls when you wire up the DB.
    /// </summary>
    public static class SessionStore
    {
        private static int _nextId = 1;
        private static readonly List<UnifiedSessionDto> _sessions = new();
        private static readonly object _lock = new();

        // ── Write ─────────────────────────────────────────────────────────────

        public static UnifiedSessionDto Add(UnifiedSessionDto session)
        {
            lock (_lock)
            {
                session.Id = _nextId++;
                _sessions.Add(session);
                return session;
            }
        }

        public static bool Update(UnifiedSessionDto session)
        {
            lock (_lock)
            {
                int idx = _sessions.FindIndex(s => s.Id == session.Id);
                if (idx < 0) return false;
                _sessions[idx] = session;
                return true;
            }
        }

        // ── Read ──────────────────────────────────────────────────────────────

        public static UnifiedSessionDto? GetById(int id)
        {
            lock (_lock) { return _sessions.FirstOrDefault(s => s.Id == id); }
        }

        public static List<UnifiedSessionDto> GetAll()
        {
            lock (_lock) { return _sessions.ToList(); }
        }

        public static List<UnifiedSessionDto> GetActive()
        {
            lock (_lock) { return _sessions.Where(s => s.Status == SessionStatus.Active).ToList(); }
        }

        public static List<UnifiedSessionDto> GetActiveForDevice(int deviceId)
        {
            lock (_lock)
            {
                return _sessions
                    .Where(s => s.SourceType == SessionSourceType.Device
                             && s.DeviceId == deviceId
                             && s.Status == SessionStatus.Active)
                    .ToList();
            }
        }

        public static List<UnifiedSessionDto> GetActiveForRoom(int roomId)
        {
            lock (_lock)
            {
                return _sessions
                    .Where(s => s.SourceType == SessionSourceType.Room
                             && s.RoomId == roomId
                             && s.Status == SessionStatus.Active)
                    .ToList();
            }
        }

        // ── Seed ──────────────────────────────────────────────────────────────

        public static void Seed()
        {
            lock (_lock)
            {
                if (_sessions.Count > 0) return;   // already seeded

                // ── Active room sessions ───────────────────────────────────────

                // Room 2 — VIP Room 2, active multi session
                _sessions.Add(new UnifiedSessionDto
                {
                    Id = _nextId++,
                    SourceType = SessionSourceType.Room,
                    RoomId = 2,
                    SourceName = "VIP Room 2",
                    SourceCategory = "Room",
                    SessionType = SessionType.Multi,
                    CustomerName = "Ahmed Ali",
                    GuestCount = 4,
                    StartTime = DateTime.UtcNow.AddMinutes(-47),
                    HourlyRate = 190.00m,
                    Status = SessionStatus.Active
                });

                // Room 5 — Premium Lounge, active single session
                _sessions.Add(new UnifiedSessionDto
                {
                    Id = _nextId++,
                    SourceType = SessionSourceType.Room,
                    RoomId = 5,
                    SourceName = "Premium Lounge",
                    SourceCategory = "Room",
                    SessionType = SessionType.Single,
                    CustomerName = "Omar Hassan",
                    GuestCount = 2,
                    StartTime = DateTime.UtcNow.AddMinutes(-23),
                    HourlyRate = 150.00m,
                    Status = SessionStatus.Active
                });

                // ── FIX: Seed dummy historical sessions into the store ─────────
                // Previously GenerateDummyHistoricalSessions() was called inside
                // SessionService.GetPage() and the results were NEVER stored here.
                // That meant GetById() always returned null for any dummy session,
                // causing "view details", "end session", and "print receipt" to
                // fail with 404 for every historical row shown in the table.
                //
                // Fix: generate them once here so they actually live in the store
                // and are retrievable by ID for the lifetime of the process.
                var dummy = GenerateDummyHistoricalSessions();
                _sessions.AddRange(dummy);

                // Keep _nextId ahead of all seeded IDs so new sessions never collide
                if (_sessions.Count > 0)
                    _nextId = Math.Max(_nextId, _sessions.Max(s => s.Id) + 1);
            }
        }

        // ── Dummy historical data (delete when EF Core is wired) ─────────────
        // Moved here from SessionService so Seed() can store them properly.

        private static List<UnifiedSessionDto> GenerateDummyHistoricalSessions()
        {
            var sessions = new List<UnifiedSessionDto>();
            var rng = new Random(42);  // fixed seed → same data every restart

            string[] deviceNames =
            {
                "PS5 - Station 1", "PS5 - Station 2",
                "PS4 - Station 1", "PS4 - Station 2",
                "Xbox Series X",   "Xbox One",
                "Gaming PC - Ultimate", "Gaming PC - Standard"
            };

            string?[] customerNames =
            {
                "Ahmed Khaled", "Mohamed Hassan", "Omar Ali",    "Youssef Mahmoud",
                "Mahmoud Ibrahim", "Ali Ahmed",   "Hassan Mohamed", null,
                "Karim Hesham",  "Tamer Said",    null,           "Amr Khaled"
            };

            // Start dummy IDs well above the active session IDs (which start at 1)
            // so they never collide. _nextId is NOT used here because we're inside
            // the lock but before _nextId is bumped at the end of Seed().
            int id = 1_000;

            for (int daysAgo = 0; daysAgo < 30; daysAgo++)
            {
                int count = rng.Next(5, 15);

                for (int i = 0; i < count; i++)
                {
                    var name = deviceNames[rng.Next(deviceNames.Length)];
                    var startTime = DateTime.Today
                        .AddDays(-daysAgo)
                        .AddHours(rng.Next(10, 22))
                        .AddMinutes(rng.Next(0, 60));
                    var duration = TimeSpan.FromMinutes(rng.Next(30, 240));

                    decimal rate = name.Contains("PS5") ? 50m
                                 : name.Contains("Xbox Series") ? 45m
                                 : name.Contains("PC - Ultimate") ? 60m
                                 : name.Contains("PC") ? 55m
                                 : 40m;

                    sessions.Add(new UnifiedSessionDto
                    {
                        Id = id++,
                        SourceType = SessionSourceType.Device,
                        SourceName = name,
                        SourceCategory = name.Contains("PS5") ? "PS5"
                                       : name.Contains("PS4") ? "PS4"
                                       : name.Contains("Xbox") ? "Xbox"
                                       : name.Contains("PC") ? "PC" : "Other",
                        CustomerName = customerNames[rng.Next(customerNames.Length)],
                        SessionType = SessionType.Single,
                        GuestCount = 1,
                        StartTime = startTime,
                        EndTime = startTime + duration,
                        HourlyRate = rate,
                        TotalCost = Math.Round((decimal)duration.TotalHours * rate, 2),
                        Status = SessionStatus.Completed,
                        PaymentMethod = rng.Next(2) == 0
                            ? PaymentMethod.Cash
                            : PaymentMethod.Card
                    });
                }
            }

            return sessions;
        }
    }
}