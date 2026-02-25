using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers;
using Station_Pro.Controllers.Station_Pro.Controllers;
using StationPro.Application.DTOs;
using StationPro.Web.Controllers;

namespace StationPro.Controllers
{
    public class SessionController : Controller
    {
        // Use the same completed sessions from DashboardController
        private static List<SessionReportDto> GetAllSessions()
        {
            var completedSessions = DashboardController.GetCompletedSessions();
            var allSessions = new List<SessionReportDto>(completedSessions);

            // Add some dummy historical data
            allSessions.AddRange(GenerateDummyHistoricalSessions());

            return allSessions;
        }

        // Main Sessions Index Page
        public IActionResult Index(string dateFilter = "today", string status = "all", int? deviceId = null, string search = "", int page = 1)
        {
            var allSessions = GetAllSessions();

            // ── Active device sessions (from Dashboard) ──────────────────────
            var activeSessions = GetActiveSessionsFromDashboard();
            var activeDeviceSessions = activeSessions.Select(session => new SessionReportDto
            {
                DeviceName = session.DeviceName,
                DeviceType = session.DeviceName.Contains("PS5") ? "PS5" :
                             session.DeviceName.Contains("PS4") ? "PS4" :
                             session.DeviceName.Contains("Xbox") ? "Xbox" :
                             session.DeviceName.Contains("PC") ? "PC" : "Other",
                CustomerName = session.CustomerName,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Status = session.Status,
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                Duration = session.Duration,
                DurationFormatted = session.DurationFormatted,
                Id = session.Id,
                SessionType = session.SessionType,
            });
            allSessions.AddRange(activeDeviceSessions);

            // ── Active room sessions (from RoomController) ───────────────────
            var activeRoomSessions = RoomController.GetActiveSessions();
            var activeRoomReports = activeRoomSessions.Select(s =>
            {
                var elapsed = DateTime.UtcNow - s.StartTime;
                return new SessionReportDto
                {
                    Id = s.Id,
                    DeviceName = RoomController.GetRoomName(s.RoomId),
                    DeviceType = "Room",
                    CustomerName = s.ClientName,
                    StartTime = s.StartTime,
                    EndTime = null,
                    Status = "Active",
                    HourlyRate = s.HourlyRate,
                    TotalCost = Math.Round((decimal)elapsed.TotalHours * s.HourlyRate, 2),
                    Duration = elapsed,
                    DurationFormatted = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}",
                    SessionType = "Room",
                };
            });
            allSessions.AddRange(activeRoomReports);

            // Apply filters
            var filteredSessions = ApplyFilters(allSessions, dateFilter, status, deviceId, search);

            // Pagination
            int pageSize = 15;
            var totalSessions = filteredSessions.Count;
            var totalPages = (int)Math.Ceiling(totalSessions / (double)pageSize);
            var paginatedSessions = filteredSessions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Calculate statistics
            var stats = new SessionStatisticsDto
            {
                TotalSessions = filteredSessions.Count,
                ActiveSessions = filteredSessions.Count(s => s.Status == "Active"),
                CompletedSessions = filteredSessions.Count(s => s.Status == "Completed"),
                TotalRevenue = filteredSessions.Sum(s => s.TotalCost),
                AverageDuration = filteredSessions.Any() ?
                    TimeSpan.FromMinutes(filteredSessions.Average(s => s.Duration.TotalMinutes)) : TimeSpan.Zero,
                MostPopularDevice = filteredSessions
                    .GroupBy(s => s.DeviceName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "N/A"
            };

            var viewModel = new SessionPageViewModel
            {
                Sessions = paginatedSessions,
                Statistics = stats,
                CurrentPage = page,
                TotalPages = totalPages,
                DateFilter = dateFilter,
                StatusFilter = status,
                DeviceFilter = deviceId,
                SearchQuery = search,
                AvailableDevices = GetAvailableDevices()
            };

            return View(viewModel);
        }

        // Session Details Modal
        public IActionResult Details(int id)
        {
            var allSessions = GetAllSessions();

            // Active device sessions
            var activeSessions = GetActiveSessionsFromDashboard();
            var activeDeviceSessions = activeSessions.Select(session => new SessionReportDto
            {
                DeviceName = session.DeviceName,
                Duration = session.Duration,
                CustomerName = session.CustomerName,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Status = session.Status,
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                DurationFormatted = session.DurationFormatted,
                Id = session.Id,
                SessionType = session.SessionType,
            });
            allSessions.AddRange(activeDeviceSessions);

            // Active room sessions
            var activeRoomSessions = RoomController.GetActiveSessions();
            var activeRoomReports = activeRoomSessions.Select(s =>
            {
                var elapsed = DateTime.UtcNow - s.StartTime;
                return new SessionReportDto
                {
                    Id = s.Id,
                    DeviceName = RoomController.GetRoomName(s.RoomId),
                    DeviceType = "Room",
                    CustomerName = s.ClientName,
                    StartTime = s.StartTime,
                    Status = "Active",
                    HourlyRate = s.HourlyRate,
                    TotalCost = Math.Round((decimal)elapsed.TotalHours * s.HourlyRate, 2),
                    Duration = elapsed,
                    DurationFormatted = $"{(int)elapsed.TotalHours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}",
                    SessionType = "Room",
                };
            });
            allSessions.AddRange(activeRoomReports);

            var session = allSessions.FirstOrDefault(s => s.Id == id);

            if (session == null)
                return NotFound();

            return PartialView("_SessionDetails", session);
        }

        // Reprint Receipt
        public IActionResult Receipt(int id)
        {
            var allSessions = GetAllSessions();
            var session = allSessions.FirstOrDefault(s => s.Id == id);

            if (session == null)
                return NotFound();

            var receipt = new SessionReceiptDto
            {
                SessionId = session.Id,
                DeviceName = session.DeviceName,
                CustomerName = session.CustomerName ?? "Guest",
                StartTime = session.StartTime,
                EndTime = session.EndTime ?? DateTime.Now,
                Duration = session.Duration,
                DurationFormatted = session.DurationFormatted,
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                PaymentMethod = session.PaymentMethod,
                CompletedAt = session.EndTime ?? DateTime.Now
            };

            return PartialView("_SessionReceipt", receipt);
        }

        [HttpPost]
        public IActionResult Start(int deviceId, string? customerName, string? customerPhone, string sessionType = "single")
        {
            try
            {
                var device = DeviceController._devices.FirstOrDefault(d => d.Id == deviceId);

                if (device == null)
                    return BadRequest(new { success = false, message = "Device not found" });

                if (!device.IsAvailable)
                    return BadRequest(new { success = false, message = "Device is not available" });

                decimal hourlyRate = sessionType == "multi" && device.MultiSessionRate.HasValue
                    ? device.MultiSessionRate.Value
                    : device.SingleSessionRate;

                int newSessionId = DashboardController.GetActiveSessions().Any()
                    ? DashboardController.GetActiveSessions().Max(s => s.Id) + 1
                    : 1;

                var newSession = new SessionDto
                {
                    Id = newSessionId,
                    DeviceId = deviceId,
                    DeviceName = device.Name,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    StartTime = DateTime.Now,
                    HourlyRate = hourlyRate,
                    Status = "Active",
                    Duration = TimeSpan.Zero,
                    TotalCost = 0,
                    SessionType = sessionType
                };

                DashboardController.AddActiveSession(newSession);

                device.IsAvailable = false;
                device.Status = sessionType == "multi" ? "In Use (Multi)" : "In Use";
                device.CurrentSession = newSession;

                return Ok(new
                {
                    success = true,
                    message = "Session started successfully",
                    sessionId = newSessionId,
                    sessionType = sessionType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "An error occurred while starting the session",
                    error = ex.Message
                });
            }
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private List<SessionReportDto> ApplyFilters(List<SessionReportDto> sessions, string dateFilter, string status, int? deviceId, string search)
        {
            var filtered = sessions.AsQueryable();

            filtered = dateFilter switch
            {
                "today" => filtered.Where(s => s.StartTime.Date == DateTime.Today),
                "yesterday" => filtered.Where(s => s.StartTime.Date == DateTime.Today.AddDays(-1)),
                "week" => filtered.Where(s => s.StartTime >= DateTime.Today.AddDays(-7)),
                "month" => filtered.Where(s => s.StartTime >= DateTime.Today.AddMonths(-1)),
                _ => filtered
            };

            if (status != "all")
                filtered = filtered.Where(s => s.Status.ToLower() == status.ToLower());

            if (deviceId.HasValue)
                filtered = filtered.Where(s => s.DeviceName.Contains($"Station {deviceId}") ||
                                               s.DeviceName.Contains($"PC {deviceId}"));

            if (!string.IsNullOrWhiteSpace(search))
                filtered = filtered.Where(s =>
                    (s.CustomerName != null && s.CustomerName.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    s.DeviceName.Contains(search, StringComparison.OrdinalIgnoreCase));

            return filtered.OrderByDescending(s => s.StartTime).ToList();
        }

        private List<DeviceDto> GetAvailableDevices()
        {
            return DeviceController._devices;
        }

        private static List<SessionReportDto> GenerateDummyHistoricalSessions()
        {
            var sessions = new List<SessionReportDto>();
            var random = new Random();
            var deviceNames = new[]
            {
                "PS5 - Station 1", "PS5 - Station 2", "PS4 - Station 1", "PS4 - Station 2",
                "Xbox Series X", "Xbox One", "Gaming PC - Ultimate", "Gaming PC - Standard"
            };
            var customerNames = new[]
            {
                "Ahmed Khaled", "Mohamed Hassan", "Omar Ali", "Youssef Mahmoud",
                "Mahmoud Ibrahim", "Ali Ahmed", "Hassan Mohamed", null, "Karim Hesham",
                "Tamer Said", null, "Amr Khaled"
            };

            int sessionId = 100;

            for (int daysAgo = 0; daysAgo < 30; daysAgo++)
            {
                var sessionsPerDay = random.Next(5, 15);

                for (int i = 0; i < sessionsPerDay; i++)
                {
                    var deviceName = deviceNames[random.Next(deviceNames.Length)];
                    var startTime = DateTime.Today.AddDays(-daysAgo)
                        .AddHours(random.Next(10, 22))
                        .AddMinutes(random.Next(0, 60));

                    var durationMinutes = random.Next(30, 240);
                    var duration = TimeSpan.FromMinutes(durationMinutes);
                    var endTime = startTime.Add(duration);

                    var hourlyRate = deviceName.Contains("PS5") ? 50m :
                                   deviceName.Contains("Xbox Series") ? 45m :
                                   deviceName.Contains("PC - Ultimate") ? 60m :
                                   deviceName.Contains("PC") ? 55m : 40m;

                    var totalCost = (decimal)(duration.TotalHours * (double)hourlyRate);

                    sessions.Add(new SessionReportDto
                    {
                        Id = sessionId++,
                        DeviceName = deviceName,
                        DeviceType = GetDeviceTypeFromName(deviceName),
                        CustomerName = customerNames[random.Next(customerNames.Length)],
                        StartTime = startTime,
                        EndTime = endTime,
                        Duration = duration,
                        DurationFormatted = $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}",
                        HourlyRate = hourlyRate,
                        TotalCost = totalCost,
                        Status = "Completed",
                        PaymentMethod = random.Next(2) == 0 ? "Cash" : "Card"
                    });
                }
            }

            return sessions;
        }

        private static string GetDeviceTypeFromName(string deviceName)
        {
            if (deviceName.Contains("PS5")) return "PS5";
            if (deviceName.Contains("PS4")) return "PS4";
            if (deviceName.Contains("Xbox")) return "Xbox";
            if (deviceName.Contains("PC")) return "PC";
            return "Other";
        }

        private List<SessionDto> GetActiveSessionsFromDashboard()
        {
            return DashboardController.GetActiveSessions();
        }
    }

    // ============================================
    // VIEW MODELS & DTOS
    // ============================================

    public class SessionPageViewModel
    {
        public List<SessionReportDto> Sessions { get; set; } = new();
        public SessionStatisticsDto Statistics { get; set; } = new();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string DateFilter { get; set; } = "today";
        public string StatusFilter { get; set; } = "all";
        public int? DeviceFilter { get; set; }
        public string SearchQuery { get; set; } = "";
        public List<DeviceDto> AvailableDevices { get; set; } = new();
    }
}