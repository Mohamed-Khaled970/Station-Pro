using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers;
using Station_Pro.Controllers.Station_Pro.Controllers;
using StationPro.Application.DTOs;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Application.Interfaces;
using StationPro.Web.Controllers;
using StationPro.Filters;
using StationPro.Domain.Entities;
using StationPro.Application.Enums;
using StationPro.Application.Contracts.Services;
using StationPro.Infrastructure.Helpers;


namespace StationPro.Controllers
{
    [SubscriptionRequired]
    public class SessionController : Controller
    {
        private readonly Application.Contracts.Services.ISessionService _sessions;
        private readonly IDeviceService _devices;

        public SessionController(Application.Contracts.Services.ISessionService sessions, IDeviceService devices)
        {
            _sessions = sessions;
            _devices = devices;
        }

        // ── Sessions list page ────────────────────────────────────────────────

        public async Task<IActionResult> Index(
            string dateFilter = "today",
            string status = "all",
            int? deviceId = null,
            string search = "",
            int page = 1)
        {
            try
            {
                // FIX: Compute Egypt-local UTC boundaries and fetch ALL sessions,
                // then filter in memory by the correct UTC window.
                //
                // WHY: Passing dateFilter = "today" to GetPageAsync uses the
                // server's UTC DateTime.Now.Date for the boundary, which equals
                // midnight UTC = 2:00 AM Egypt time.  Any session started before
                // 2:00 AM Egypt local time therefore lands in the server's
                // "yesterday" bucket, causing newly started sessions to disappear
                // from "today" and appear in "yesterday" instead.
                var (startUtc, endUtc) = TimeZoneHelper.GetUtcDateRange(dateFilter);

                var filter = new SessionFilterRequest
                {
                    DateFilter = "all",   // fetch all; we apply the Egypt-correct window below
                    Status = status,
                    DeviceId = deviceId,
                    Search = search,
                    Page = 1,
                    PageSize = 10_000   // get everything, paginate after filtering
                };

                var result = await _sessions.GetPageAsync(filter);
                var availableDevices = await _devices.GetAllAsync();

                // Apply the Egypt-local date window
                var filtered = result.Sessions?
                    .Where(s => s.StartTime >= startUtc && s.StartTime <= endUtc)
                    .ToList() ?? new List<UnifiedSessionDto>();

                // Re-paginate after filtering
                const int pageSize = 20;
                int totalFiltered = filtered.Count;
                int totalPages = (int)Math.Ceiling(totalFiltered / (double)pageSize);
                int safePage = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));
                var pagedSessions = filtered
                    .Skip((safePage - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Recompute statistics from the filtered set
                var stats = BuildStatistics(filtered);

                var viewModel = new SessionPageViewModel
                {
                    Sessions = pagedSessions,
                    Statistics = stats,
                    CurrentPage = safePage,
                    TotalPages = totalPages,
                    DateFilter = dateFilter,
                    StatusFilter = status,
                    DeviceFilter = deviceId,
                    SearchQuery = search,
                    AvailableDevices = availableDevices?.ToList() ?? new List<DeviceDto>()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[SessionController.Index] {ex}");

                List<DeviceDto> devices = new();
                try { devices = (await _devices.GetAllAsync())?.ToList() ?? devices; } catch { /* ignore */ }

                return View(new SessionPageViewModel
                {
                    Sessions = new List<UnifiedSessionDto>(),
                    Statistics = new SessionStatisticsDto { AverageDuration = TimeSpan.Zero, MostPopularDevice = "—" },
                    DateFilter = dateFilter,
                    StatusFilter = status,
                    DeviceFilter = deviceId,
                    SearchQuery = search,
                    AvailableDevices = devices
                });
            }
        }

        // ── Build statistics from an already-filtered list ────────────────────

        private static SessionStatisticsDto BuildStatistics(List<UnifiedSessionDto> sessions)
        {
            var completed = sessions.Where(s => s.Status == SessionStatus.Completed).ToList();

            return new SessionStatisticsDto
            {
                TotalSessions = sessions.Count,
                TotalRevenue = completed.Sum(s => s.TotalCost),
                AverageDuration = completed.Any()
                    ? TimeSpan.FromMinutes(completed.Average(s => s.Duration.TotalMinutes))
                    : TimeSpan.Zero,
                MostPopularDevice = completed
                    .GroupBy(s => s.SourceName)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? "—"
            };
        }

        // ── Session details modal ─────────────────────────────────────────────

        public async Task<IActionResult> Details(int id)
        {
            var session = await _sessions.GetByIdAsync(id);
            if (session == null) return NotFound();

            var report = new SessionReportDto
            {
                Id = session.Id,
                DeviceName = session.SourceName,
                DeviceType = session.SourceCategory ?? string.Empty,
                CustomerName = session.CustomerName,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Duration = session.Duration,
                DurationFormatted = session.DurationFormatted,
                HourlyRate = session.HourlyRate,
                TotalCost = session.IsActive
                    ? Math.Round((decimal)session.Duration.TotalHours * session.HourlyRate, 2)
                    : session.TotalCost,
                Status = session.StatusString,
                PaymentMethod = session.PaymentMethod?.ToString() ?? "Cash",
                SessionType = session.SessionTypeString
            };

            return PartialView("_SessionDetails", report);
        }

        // ── Print receipt ─────────────────────────────────────────────────────

        public async Task<IActionResult> Receipt(int id)
        {
            var receipt = await _sessions.GetReceiptAsync(id);
            if (receipt == null) return NotFound();
            return PartialView("_SessionReceipt", receipt);
        }

        // ── Start a device session ────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Start(
            int deviceId,
            string? customerName,
            string? customerPhone,
            string sessionType = "single")
        {
            try
            {
                var request = new StartDeviceSessionRequest
                {
                    DeviceId = deviceId,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    SessionType = sessionType
                };

                var result = await _sessions.StartDeviceSessionAsync(request, deviceId);

                var device = await _devices.GetByIdAsync(deviceId);
                var triggerData = new
                {
                    sessionStarted = new
                    {
                        deviceName = device?.Name ?? string.Empty,
                        sessionType = result.SessionType,
                        rate = result.HourlyRate
                    }
                };

                Response.Headers.Append(
                    "HX-Trigger",
                    System.Text.Json.JsonSerializer.Serialize(triggerData));

                return Ok(new { success = true, message = "Session started successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ── End a session (device or room) ────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> End(int sessionId, int paymentMethod = 1)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null || !session.IsActive)
                return NotFound(new { success = false, message = "Active session not found." });

            try
            {
                if (session.SourceType == SessionSourceType.Device)
                    await _sessions.EndDeviceSessionAsync(sessionId, paymentMethod);
                else if (session.SourceType == SessionSourceType.Room)
                    await _sessions.EndRoomSessionAsync(sessionId);
                else
                    return BadRequest(new { success = false, message = "Unknown session type." });

                var receipt = await _sessions.GetReceiptAsync(sessionId);
                return PartialView("~/Views/Shared/_SessionReceipt.cshtml", receipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    // ============================================
    // VIEW MODELS & DTOS
    // ============================================

    public class SessionPageViewModel
    {
        public List<UnifiedSessionDto> Sessions { get; set; } = new();
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