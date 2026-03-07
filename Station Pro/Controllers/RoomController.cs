using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers.Station_Pro.Controllers;
using StationPro.Application.DTOs;
using StationPro.Application.Enums;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Application.Interfaces;
using StationPro.Filters;

namespace Station_Pro.Controllers
{
    [SubscriptionRequired]
    public class RoomController : Controller
    {
        private readonly ISessionService _sessions;

        public RoomController(ISessionService sessions)
        {
            _sessions = sessions;
        }

        // ─── Static helpers (called from SessionController) ────────────────────

        public static List<UnifiedSessionDto> GetActiveSessions()
            => SessionStore.GetActive()
               .Where(s => s.SourceType == SessionSourceType.Room)
               .ToList();

        public static string GetRoomName(int roomId)
            => RoomStore.GetName(roomId);

        // ─── View ─────────────────────────────────────────────────────────────

        public IActionResult Index()
            => View(RoomStore.GetAll());

        // ─── Room CRUD ────────────────────────────────────────────────────────

        [HttpPost]
        public IActionResult Create(RoomDto room)
        {
            // Back-compat: if old single-rate form posts HourlyRate, mirror it
            if (room.SingleHourlyRate == 0 && room.HourlyRate > 0)
                room.SingleHourlyRate = room.HourlyRate;
            if (room.MultiHourlyRate == 0 && room.SingleHourlyRate > 0)
                room.MultiHourlyRate = room.SingleHourlyRate;

            var created = RoomStore.Add(room);
            return PartialView("_RoomCard", created);
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            var room = RoomStore.GetById(id);
            return room == null ? NotFound() : Json(room);
        }

        [HttpPut]
        public IActionResult Update(int id, [FromBody] RoomDto updated)
        {
            var room = RoomStore.GetById(id);
            if (room == null) return NotFound();

            room.Name = updated.Name;
            room.HasAC = updated.HasAC;
            room.SingleHourlyRate = updated.SingleHourlyRate;
            room.MultiHourlyRate = updated.MultiHourlyRate;
            room.HourlyRate = updated.SingleHourlyRate;   // keep legacy in sync
            room.Capacity = updated.Capacity;
            room.DeviceCount = updated.DeviceCount;
            room.IsActive = updated.IsActive;

            RoomStore.Update(room);
            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var room = RoomStore.GetById(id);
            if (room == null) return NotFound();

            if (room.Status == "Occupied")
                return BadRequest(new { message = "Cannot delete an occupied room. End the session first." });

            RoomStore.Delete(id);
            return Ok();
        }

        // ─── Session Management ───────────────────────────────────────────────

        [HttpPost]
        public IActionResult StartSession([FromBody] StartRoomSessionRequest request)
        {
            var room = RoomStore.GetById(request.RoomId);
            if (room == null) return NotFound(new { message = "Room not found." });

            try
            {
                var result = _sessions.StartRoomSession(request, room);
                RoomStore.Update(room);   // persist updated room state
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult EndSession(int sessionId)
        {
            var session = _sessions.GetById(sessionId);
            if (session == null || !session.IsActive)
                return NotFound(new { message = "Active session not found." });

            var room = RoomStore.GetById(session.RoomId!.Value);
            if (room == null) return NotFound(new { message = "Room not found." });

            try
            {
                var result = _sessions.EndRoomSession(sessionId, room);
                RoomStore.Update(room);   // persist reset room state
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetSession(int sessionId)
        {
            var session = _sessions.GetById(sessionId);
            if (session == null) return NotFound();

            return Json(new
            {
                id = session.Id,
                roomId = session.RoomId,
                clientName = session.CustomerName,
                guestCount = session.GuestCount,
                sessionType = session.SessionType.ToString(),
                startTime = session.StartTime.ToString("O"),
                hourlyRate = session.HourlyRate,
                currentCost = session.RunningCost,
                isActive = session.IsActive
            });
        }

        // ─── Reservation Management ───────────────────────────────────────────

        [HttpPost]
        public IActionResult Reserve([FromBody] CreateReservationRequest request)
        {
            var room = RoomStore.GetById(request.RoomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Available")
                return BadRequest(new { message = $"Room is currently {room.Status}." });

            var reservation = RoomStore.AddReservation(new RoomReservationDto
            {
                RoomId = room.Id,
                ClientName = request.ClientName,
                Phone = request.Phone,
                ReservationTime = request.ReservationTime,
                Notes = request.Notes
            });

            room.Status = "Reserved";
            room.ReservationClientName = request.ClientName;
            room.ReservationTime = request.ReservationTime;
            room.ReservationNotes = request.Notes;
            RoomStore.Update(room);

            return Ok(new { success = true, reservationId = reservation.Id });
        }

        [HttpPost]
        public IActionResult CancelReservation(int roomId)
        {
            var room = RoomStore.GetById(roomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Reserved")
                return BadRequest(new { message = "Room has no active reservation." });

            RoomStore.RemoveReservation(roomId);

            room.Status = "Available";
            room.ReservationClientName = null;
            room.ReservationTime = null;
            room.ReservationNotes = null;
            RoomStore.Update(room);

            return Ok(new { success = true });
        }

        [HttpPost]
        public IActionResult ActivateReservation(int roomId)
        {
            var room = RoomStore.GetById(roomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Reserved")
                return BadRequest(new { message = "Room is not reserved." });

            var reservation = RoomStore.GetReservation(roomId);
            if (reservation == null) return NotFound(new { message = "Reservation not found." });

            var request = new StartRoomSessionRequest
            {
                RoomId = roomId,
                ClientName = reservation.ClientName,
                GuestCount = 1,
                SessionType = "Single"
            };

            // ✅ FIX: clear reservation state BEFORE StartRoomSession checks room.Status
            room.Status = "Available";
            room.ReservationClientName = null;
            room.ReservationTime = null;
            room.ReservationNotes = null;
            RoomStore.RemoveReservation(roomId);

            try
            {
                var result = _sessions.StartRoomSession(request, room);
                RoomStore.Update(room);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // ✅ FIX: rollback room status if session start fails
                room.Status = "Available";
                RoomStore.Update(room);
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── Card Partial & Receipt ───────────────────────────────────────────

        [HttpGet]
        public IActionResult CardPartial(int id)
        {
            var room = RoomStore.GetById(id);
            return room == null ? NotFound() : PartialView("_RoomCard", room);
        }

        [HttpGet]
        public IActionResult SessionReceipt(int sessionId)
        {
            var receipt = _sessions.GetReceipt(sessionId);
            if (receipt == null) return NotFound();
            return PartialView("~/Views/Shared/_SessionReceipt.cshtml", receipt);
        }
    }
}