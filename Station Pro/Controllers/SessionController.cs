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

namespace StationPro.Controllers
{
    [SubscriptionRequired]
    public class SessionController : Controller
    {
        private readonly ISessionService _sessions;

        public SessionController(ISessionService sessions)
        {
            _sessions = sessions;
        }

        // ─── Sessions list page ───────────────────────────────────────────────

        public IActionResult Index(
            string dateFilter = "today",
            string status = "all",
            int? deviceId = null,
            string search = "",
            int page = 1)
        {
            var filter = new SessionFilterRequest
            {
                DateFilter = dateFilter,
                Status = status,
                DeviceId = deviceId,
                Search = search,
                Page = page
            };

            var result = _sessions.GetPage(filter);

            var viewModel = new SessionPageViewModel
            {
                Sessions = result.Sessions,
                Statistics = result.Statistics,
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                DateFilter = dateFilter,
                StatusFilter = status,
                DeviceFilter = deviceId,
                SearchQuery = search,
                AvailableDevices = DeviceStore.GetAll()
            };

            return View(viewModel);
        }

        // ─── Session details modal ────────────────────────────────────────────

        public IActionResult Details(int id)
        {
            var session = _sessions.GetById(id);
            if (session == null) return NotFound();

            // ✅ FIX: _SessionDetails expects SessionReportDto, not UnifiedSessionDto
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
                TotalCost = session.Status == SessionStatus.Active
                    ? Math.Round((decimal)session.Duration.TotalHours * session.HourlyRate, 2)
                    : session.TotalCost,
                Status = session.Status.ToString(),
                PaymentMethod = session.PaymentMethod?.ToString() ?? "Cash",
                SessionType = session.SessionType.ToString()
            };

            return PartialView("_SessionDetails", report);
        }

        // ─── Print receipt ────────────────────────────────────────────────────

        public IActionResult Receipt(int id)
        {
            var receipt = _sessions.GetReceipt(id);
            if (receipt == null) return NotFound();
            return PartialView("_SessionReceipt", receipt);
        }

        // ─── Start a device session ───────────────────────────────────────────

        [HttpPost]
        public IActionResult Start(
            int deviceId,
            string? customerName,
            string? customerPhone,
            string sessionType = "single")
        {
            var device = DeviceStore.GetById(deviceId);
            if (device == null)
                return BadRequest(new { success = false, message = "Device not found." });

            if (!device.IsAvailable)
                return BadRequest(new { success = false, message = "Device is not available." });

            try
            {
                var request = new StartDeviceSessionRequest
                {
                    DeviceId = deviceId,
                    CustomerName = customerName,
                    CustomerPhone = customerPhone,
                    SessionType = sessionType
                };

                var result = _sessions.StartDeviceSession(request, device);
                DeviceStore.Update(device);   // persist updated device state

                return Ok(new
                {
                    result.Success,
                    message = "Session started successfully",
                    result.SessionId,
                    result.SessionType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public IActionResult End(int sessionId, int paymentMethod = 1)
        {
            var session = _sessions.GetById(sessionId);
            if (session == null || !session.IsActive)
                return NotFound(new { success = false, message = "Active session not found." });

            try
            {
                if (session.SourceType == SessionSourceType.Device)
                {
                    var device = DeviceStore.GetById(session.DeviceId!.Value);
                    if (device == null)
                        return NotFound(new { success = false, message = "Device not found." });

                    _sessions.EndDeviceSession(sessionId, paymentMethod, device);
                    DeviceStore.Update(device);
                }
                else if (session.SourceType == SessionSourceType.Room)
                {
                    var room = RoomStore.GetById(session.RoomId!.Value);
                    if (room == null)
                        return NotFound(new { success = false, message = "Room not found." });

                    _sessions.EndRoomSession(sessionId, room);
                    RoomStore.Update(room);
                }
                else
                {
                    return BadRequest(new { success = false, message = "Unknown session type." });
                }

                var receipt = _sessions.GetReceipt(sessionId);
                return PartialView("~/Views/Shared/_SessionReceipt.cshtml", receipt);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
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