using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Application.Interfaces;
using StationPro.Domain.Entities;
using StationPro.Application.Enums;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Filters;
using StationPro.Application.Contracts.Services;
using StationPro.Infrastructure.Helpers;

namespace Station_Pro.Controllers.Station_Pro.Controllers
{
    [SubscriptionRequired]
    public class DashboardController : Controller
    {
        private readonly StationPro.Application.Contracts.Services.ISessionService _sessions;
        private readonly IDeviceService _devices;

        public DashboardController(
            StationPro.Application.Contracts.Services.ISessionService sessions,
            IDeviceService devices)
        {
            _sessions = sessions;
            _devices = devices;
        }

        public static List<UnifiedSessionDto> GetCompletedSessions()
            => SessionStore.GetAll()
               .Where(s => s.Status == SessionStatus.Completed)
               .ToList();

        public async Task<IActionResult> Index()
        {
            var stats = await GetEgyptAwareStatsAsync();
            return View(stats);
        }

        public async Task<IActionResult> LiveStats()
        {
            var stats = await GetEgyptAwareStatsAsync();
            return PartialView("_DashboardStats", stats);
        }

        /// <summary>
        /// FIX: Builds dashboard stats using Egypt-local "today" boundaries.
        ///
        /// WHY the old version showed $0.00 revenue:
        /// GetDashboardStatsAsync() calculated "today" using DateTime.Now.Date
        /// on the UTC server, so "today" started at UTC midnight = 2:00 AM Egypt.
        /// All sessions completed before 2 AM Egypt were excluded from "today"
        /// even though they happened on today's Egypt calendar date, making
        /// TodayRevenue and TotalSessions always 0 or incorrect.
        /// </summary>
        private async Task<DashboardStatsDto> GetEgyptAwareStatsAsync()
        {
            var (todayStartUtc, todayEndUtc) = TimeZoneHelper.GetUtcDateRange("today");

            // Fetch the base stats from the service
            var stats = await _sessions.GetDashboardStatsAsync();

            // Re-compute today-specific figures using the Egypt-correct window
            var allSessions = await _sessions.GetPageAsync(new SessionFilterRequest
            {
                DateFilter = "all",
                Status = "completed",
                Page = 1,
                PageSize = 10_000
            });

            var todaySessions = allSessions.Sessions?
                .Where(s => s.StartTime >= todayStartUtc && s.StartTime <= todayEndUtc)
                .ToList() ?? new();

            stats.TodayRevenue = todaySessions.Sum(s => s.TotalCost);
            stats.TotalSessions = todaySessions.Count;
            stats.AverageSessionCost = todaySessions.Any()
                ? todaySessions.Average(s => s.TotalCost)
                : 0;

            return stats;
        }

        public async Task<IActionResult> ActiveSessions()
        {
            var active = await _sessions.GetActiveForDevicesAsync();
            return PartialView("_ActiveSessions", active);
        }

        public async Task<IActionResult> DeviceCards()
        {
            var devices = await _devices.GetAllWithActiveSessionsAsync();
            return PartialView("_DeviceCards", devices);
        }

        [HttpGet]
        public async Task<IActionResult> DeviceOptions()
        {
            var devices = await _devices.GetAllWithActiveSessionsAsync();
            var available = devices
                .Where(d => d.Status == "Available")
                .Select(d => new { d.Id, d.Name })
                .OrderBy(d => d.Name);
            return Json(available);
        }

        [HttpPost]
        public async Task<IActionResult> End(int sessionId, int paymentMethod = 1)
        {
            try
            {
                await _sessions.EndDeviceSessionAsync(sessionId, paymentMethod);
                var receipt = await _sessions.GetReceiptAsync(sessionId);
                if (receipt == null)
                    return NotFound(new { success = false, message = "Receipt not found." });
                return PartialView("_SessionReceipt", receipt);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> QuickStart(int deviceId, bool isMultiSession = false)
        {
            try
            {
                var request = new StartDeviceSessionRequest
                {
                    DeviceId = deviceId,
                    SessionType = isMultiSession ? "multi" : "single"
                };
                var result = await _sessions.StartDeviceSessionAsync(request, deviceId);
                var device = await _devices.GetByIdAsync(deviceId);
                var triggerData = new
                {
                    sessionStarted = new
                    {
                        deviceName = device?.Name ?? string.Empty,
                        sessionType = isMultiSession ? "multi" : "single",
                        rate = result.HourlyRate
                    }
                };
                Response.Headers.Append("HX-Trigger", System.Text.Json.JsonSerializer.Serialize(triggerData));
                return await ActiveSessions();
            }
            catch (InvalidOperationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { success = false, message = ex.Message }); }
        }
    }
}