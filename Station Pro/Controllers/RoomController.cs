using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers.Station_Pro.Controllers;
using StationPro.Application.DTOs;
using StationPro.Application.Enums;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Application.Interfaces;
using StationPro.Filters;
using StationPro.Application.Contracts.Services;

namespace Station_Pro.Controllers
{
    [SubscriptionRequired]
    public class RoomController : Controller
    {
        private readonly IRoomService _rooms;
        private readonly StationPro.Application.Contracts.Services.ISessionService _sessions;

        public RoomController(IRoomService rooms, StationPro.Application.Contracts.Services.ISessionService sessions)
        {
            _rooms = rooms;
            _sessions = sessions;
        }

        // ── View ──────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var rooms = await _rooms.GetAllWithActiveSessionsAsync();
            return View(rooms);
        }

        // ── Room CRUD ─────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            return room == null ? NotFound() : Json(room);
        }

        /// <summary>
        /// Called by the "Add Room" HTMX form.
        /// Returns the new _RoomCard partial so HTMX can swap it in.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] RoomDto dto)
        {
            try
            {
                var created = await _rooms.CreateAsync(dto);
                return PartialView("_RoomCard", created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update(int id, [FromBody] RoomDto dto)
        {
            try
            {
                var updated = await _rooms.UpdateAsync(id, dto);
                return Ok(updated);
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

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _rooms.DeleteAsync(id);
                return Ok(new { success = true });
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

        // ── Session Management ────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> StartSession([FromBody] StartRoomSessionRequest request)
        {
            try
            {
                var result = await _sessions.StartRoomSessionAsync(request, request.RoomId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> EndSession(int sessionId)
        {
            try
            {
                var result = await _sessions.EndRoomSessionAsync(sessionId);
                return Ok(result);
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

        [HttpGet]
        public async Task<IActionResult> GetSession(int sessionId)
        {
            var session = await _sessions.GetByIdAsync(sessionId);
            if (session == null) return NotFound();

            return Json(new
            {
                id = session.Id,
                roomId = session.RoomId,
                clientName = session.CustomerName,
                guestCount = session.GuestCount,
                sessionType = session.SessionTypeString,
                startTime = session.StartTime.ToString("O"),
                hourlyRate = session.HourlyRate,
                currentCost = session.RunningCost,
                isActive = session.IsActive
            });
        }

        // ── Reservation Management ────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Reserve([FromBody] CreateReservationRequest request)
        {
            var room = await _rooms.GetByIdAsync(request.RoomId);
            if (room == null)
                return NotFound(new { success = false, message = "Room not found." });

            if (room.Status != "Available")
                return BadRequest(new { success = false, message = $"Room is currently {room.Status}." });

            try
            {
                var reservation = await _rooms.AddReservationAsync(request);
                return Ok(new { success = true, reservationId = reservation.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelReservation(int roomId)
        {
            var room = await _rooms.GetByIdAsync(roomId);
            if (room == null)
                return NotFound(new { success = false, message = "Room not found." });

            if (room.Status != "Reserved")
                return BadRequest(new { success = false, message = "Room has no active reservation." });

            await _rooms.RemoveReservationAsync(roomId);
            return Ok(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ActivateReservation(int roomId)
        {
            var room = await _rooms.GetByIdAsync(roomId);
            if (room == null)
                return NotFound(new { success = false, message = "Room not found." });

            if (room.Status != "Reserved")
                return BadRequest(new { success = false, message = "Room is not reserved." });

            var reservation = await _rooms.GetReservationAsync(roomId);
            if (reservation == null)
                return NotFound(new { success = false, message = "Reservation not found." });

            // Remove the reservation BEFORE starting the session so
            // StartRoomSessionAsync sees the room as Available.
            await _rooms.RemoveReservationAsync(roomId);

            try
            {
                var request = new StartRoomSessionRequest
                {
                    RoomId = roomId,
                    ClientName = reservation.ClientName,
                    GuestCount = 1,
                    SessionType = "Single"
                };

                var result = await _sessions.StartRoomSessionAsync(request, roomId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Rollback: put the reservation back so the UI is consistent.
                await _rooms.AddReservationAsync(new CreateReservationRequest
                {
                    RoomId = roomId,
                    ClientName = reservation.ClientName,
                    Phone = reservation.Phone,
                    ReservationTime = reservation.ReservationTime,
                    Notes = reservation.Notes
                });

                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ── Card partial & Receipt ─────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CardPartial(int id)
        {
            var room = await _rooms.GetByIdAsync(id);
            return room == null ? NotFound() : PartialView("_RoomCard", room);
        }

        [HttpGet]
        public async Task<IActionResult> SessionReceipt(int sessionId)
        {
            var receipt = await _sessions.GetReceiptAsync(sessionId);
            if (receipt == null) return NotFound();
            return PartialView("~/Views/Shared/_SessionReceipt.cshtml", receipt);
        }
    }
}