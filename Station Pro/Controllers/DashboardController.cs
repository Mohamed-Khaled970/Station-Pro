using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Application.Interfaces;
using StationPro.Domain.Entities;

using StationPro.Application.Enums;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Filters;

namespace Station_Pro.Controllers.Station_Pro.Controllers
{
    [SubscriptionRequired]
    public class DashboardController : Controller
    {
        private readonly ISessionService _sessions;

        public DashboardController(ISessionService sessions)
        {
            _sessions = sessions;
        }

        // ─── Static helpers kept for backward compat ──────────────────────────
        // SessionController and RoomController call these.
        // They now just proxy to SessionStore instead of owning private lists.

        public static List<UnifiedSessionDto> GetActiveSessions()
            => SessionStore.GetActive()
               .Where(s => s.SourceType == SessionSourceType.Device)
               .ToList();

        public static List<UnifiedSessionDto> GetCompletedSessions()
            => SessionStore.GetAll()
               .Where(s => s.Status == SessionStatus.Completed)
               .ToList();

        // These two are kept so old call sites compile without changes.
        // They are no-ops now — SessionStore is the single store.
        public static void AddActiveSession(SessionDto session) { }
        public static void AddCompletedSession(SessionReportDto session) { }

        // ─── Views ────────────────────────────────────────────────────────────

        public IActionResult Index()
        {
            var allToday = SessionStore.GetAll()
                .Where(s => s.StartTime.Date == DateTime.Today)
                .ToList();

            var stats = new DashboardStatsDto
            {
                TodayRevenue = CalculateTodayRevenue(),
                TotalSessions = allToday.Count,
                ActiveSessions = allToday.Count(s => s.Status == SessionStatus.Active),
                ActiveDevices = allToday.Count(s => s.Status == SessionStatus.Active
                                                       && s.SourceType == SessionSourceType.Device),
                TotalDevices = DeviceStore.GetAll().Count,
                AverageSessionCost = CalculateAverageCost()
            };

            return View(stats);
        }

        public IActionResult LiveStats()
        {
            var stats = new DashboardStatsDto
            {
                TodayRevenue = CalculateTodayRevenue(),
                TotalSessions = SessionStore.GetAll().Count(s => s.StartTime.Date == DateTime.Today),
                ActiveSessions = SessionStore.GetActive().Count,
                ActiveDevices = SessionStore.GetActive().Count(s => s.SourceType == SessionSourceType.Device),
                TotalDevices = DeviceStore.GetAll().Count,
                AverageSessionCost = CalculateAverageCost()
            };

            return PartialView("_DashboardStats", stats);
        }

        public IActionResult ActiveSessions()
        {
            // Build SessionDto list from UnifiedSessionDto for the active sessions partial
            // (the _ActiveSessions partial still uses the old SessionDto model)
            var active = SessionStore.GetActive()
                .Where(s => s.SourceType == SessionSourceType.Device)
                .Select(s => new SessionDto
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
                    TotalCost = s.RunningCost,
                    SessionType = s.SessionType.ToString().ToLower()
                })
                .ToList();

            return PartialView("_ActiveSessions", active);
        }

        public IActionResult DeviceCards()
        {
            var allDevices = DeviceStore.GetAll();
            var activeSessions = SessionStore.GetActive()
                .Where(s => s.SourceType == SessionSourceType.Device)
                .ToList();

            // Attach active session snapshots to device DTOs
            foreach (var device in allDevices)
            {
                var running = activeSessions.FirstOrDefault(s => s.DeviceId == device.Id);
                if (running != null)
                {
                    device.DeviceStatus = DeviceStatus.InUse;
                    device.ActiveSessionId = running.Id;
                    device.CurrentSession = new SessionDto
                    {
                        Id = running.Id,
                        DeviceId = running.DeviceId ?? 0,
                        DeviceName = running.SourceName,
                        CustomerName = running.CustomerName,
                        CustomerPhone = running.CustomerPhone,
                        StartTime = running.StartTime,
                        HourlyRate = running.HourlyRate,
                        Status = "Active",
                        Duration = running.Duration,
                        TotalCost = running.RunningCost,
                        SessionType = running.SessionType.ToString().ToLower()
                    };
                }
                else
                {
                    device.DeviceStatus = DeviceStatus.Available;
                    device.ActiveSessionId = null;
                    device.CurrentSession = null;
                }
            }

            return PartialView("_DeviceCards", allDevices);
        }

        // ─── End session ──────────────────────────────────────────────────────

        [HttpPost]
        public IActionResult End(int sessionId, int paymentMethod = 1)
        {
            var session = _sessions.GetById(sessionId);
            if (session == null || !session.IsActive)
                return NotFound(new { success = false, message = "Active device session not found." });

            var device = DeviceStore.GetById(session.DeviceId!.Value);
            if (device == null)
                return NotFound(new { success = false, message = "Device not found." });

            try
            {
                _sessions.EndDeviceSession(sessionId, paymentMethod, device);
                DeviceStore.Update(device);

                var receipt = _sessions.GetReceipt(sessionId);
                return PartialView("_SessionReceipt", receipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ─── Quick start (from dashboard device card) ─────────────────────────

        [HttpPost]
        public IActionResult QuickStart(int deviceId, bool isMultiSession = false)
        {
            var device = DeviceStore.GetById(deviceId);
            if (device == null)
                return BadRequest(new { success = false, message = "Device not found." });

            if (!device.IsAvailable)
                return BadRequest(new { success = false, message = "Device is already in use." });

            if (isMultiSession && (!device.SupportsMultiSession || !device.MultiSessionRate.HasValue))
                return BadRequest(new { success = false, message = "Device does not support multi-session." });


            try
            {
                var request = new StartDeviceSessionRequest
                {
                    DeviceId = deviceId,
                    SessionType = isMultiSession ? "multi" : "single"
                };

                var result = _sessions.StartDeviceSession(request, device);
                DeviceStore.Update(device);

                var triggerData = new
                {
                    sessionStarted = new
                    {
                        deviceName = device.Name,
                        sessionType = isMultiSession ? "multi" : "single",
                        rate = result.HourlyRate
                    }
                };

                Response.Headers.Append(
                    "HX-Trigger",
                    System.Text.Json.JsonSerializer.Serialize(triggerData));

                // ✅ FIX: Return active sessions (correct target), not device cards
                // Device cards will refresh separately via the sessionStarted HX-Trigger event
                return ActiveSessions();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ─── Private helpers ──────────────────────────────────────────────────

        private static decimal CalculateTodayRevenue()
        {
            var today = DateTime.Today;

            var completed = SessionStore.GetAll()
                .Where(s => s.StartTime.Date == today && s.Status == SessionStatus.Completed)
                .Sum(s => s.TotalCost);

            var active = SessionStore.GetActive()
                .Where(s => s.StartTime.Date == today)
                .Sum(s => Math.Round((decimal)s.Duration.TotalHours * s.HourlyRate, 2));

            return completed + active;
        }

        private static decimal CalculateAverageCost()
        {
            var active = SessionStore.GetActive().ToList();
            if (!active.Any()) return 0;

            return active.Average(s =>
                Math.Round((decimal)s.Duration.TotalHours * s.HourlyRate, 2));
        }
    }
}