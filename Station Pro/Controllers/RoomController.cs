using Microsoft.AspNetCore.Mvc;
using Station_Pro.Controllers.Station_Pro.Controllers;
using StationPro.Application.DTOs;

namespace Station_Pro.Controllers
{
    public class RoomController : Controller
    {
        // ─── In-memory stores (replace with real DB service later) ───────────
        private static int _nextRoomId = 9;
        private static int _nextSessionId = 1;
        private static int _nextReservationId = 1;

        private static List<RoomDto> _rooms = new()
        {
            new RoomDto { Id = 1, Name = "VIP Room 1",       HasAC = true,  HourlyRate = 100.00m, Capacity = 4,  IsActive = true,  Status = "Available",    CurrentOccupancy = 0, DeviceCount = 2 },
            new RoomDto { Id = 2, Name = "VIP Room 2",       HasAC = true,  HourlyRate = 120.00m, Capacity = 6,  IsActive = true,  Status = "Occupied",     CurrentOccupancy = 4, DeviceCount = 3,
                          SessionStartTime = DateTime.UtcNow.AddMinutes(-47), SessionClientName = "Ahmed Ali", ActiveSessionId = 1 },
            new RoomDto { Id = 3, Name = "Standard Room A",  HasAC = false, HourlyRate = 60.00m,  Capacity = 3,  IsActive = true,  Status = "Available",    CurrentOccupancy = 0, DeviceCount = 1 },
            new RoomDto { Id = 4, Name = "Standard Room B",  HasAC = false, HourlyRate = 60.00m,  Capacity = 3,  IsActive = true,  Status = "Available",    CurrentOccupancy = 0, DeviceCount = 1 },
            new RoomDto { Id = 5, Name = "Premium Lounge",   HasAC = true,  HourlyRate = 150.00m, Capacity = 8,  IsActive = true,  Status = "Occupied",     CurrentOccupancy = 6, DeviceCount = 4,
                          SessionStartTime = DateTime.UtcNow.AddMinutes(-23), SessionClientName = "Omar Hassan", ActiveSessionId = 2 },
            new RoomDto { Id = 6, Name = "Party Room",       HasAC = true,  HourlyRate = 200.00m, Capacity = 10, IsActive = true,  Status = "Reserved",     CurrentOccupancy = 0, DeviceCount = 5,
                          ReservationClientName = "Sara Mohamed", ReservationTime = DateTime.UtcNow.AddHours(2), ReservationNotes = "Birthday party" },
            new RoomDto { Id = 7, Name = "Gaming Pod 1",     HasAC = false, HourlyRate = 40.00m,  Capacity = 2,  IsActive = true,  Status = "Available",    CurrentOccupancy = 0, DeviceCount = 1 },
            new RoomDto { Id = 8, Name = "Conference Room",  HasAC = true,  HourlyRate = 80.00m,  Capacity = 12, IsActive = false, Status = "Maintenance",  CurrentOccupancy = 0, DeviceCount = 2 },
        };

        private static List<RoomSessionDto> _sessions = new()
        {
            new RoomSessionDto { Id = 1, RoomId = 2, ClientName = "Ahmed Ali",   GuestCount = 4, StartTime = DateTime.UtcNow.AddMinutes(-47), HourlyRate = 120.00m, IsActive = true },
            new RoomSessionDto { Id = 2, RoomId = 5, ClientName = "Omar Hassan", GuestCount = 6, StartTime = DateTime.UtcNow.AddMinutes(-23), HourlyRate = 150.00m, IsActive = true },
        };

        private static List<RoomReservationDto> _reservations = new()
        {
            new RoomReservationDto { Id = 1, RoomId = 6, ClientName = "Sara Mohamed", Phone = "01012345678", ReservationTime = DateTime.UtcNow.AddHours(2), Notes = "Birthday party" },
        };

        // ─── Views ────────────────────────────────────────────────────────────

        public IActionResult Index()
        {
            return View(_rooms);
        }

        // ─── Room CRUD ────────────────────────────────────────────────────────

        [HttpPost]
        public IActionResult Create(RoomDto room)
        {
            room.Id = _nextRoomId++;
            room.Status = "Available";
            _rooms.Add(room);
            return PartialView("_RoomCard", room);
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();
            return Json(room);
        }

        [HttpPut]
        public IActionResult Update(int id, [FromBody] RoomDto updatedRoom)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            room.Name = updatedRoom.Name;
            room.HasAC = updatedRoom.HasAC;
            room.HourlyRate = updatedRoom.HourlyRate;
            room.Capacity = updatedRoom.Capacity;
            room.DeviceCount = updatedRoom.DeviceCount;
            room.IsActive = updatedRoom.IsActive;

            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            if (room.Status == "Occupied")
                return BadRequest(new { message = "Cannot delete an occupied room. End the session first." });

            _rooms.Remove(room);
            return Ok();
        }

        // ─── Session Management ───────────────────────────────────────────────

        /// <summary>Start a room session → room becomes Occupied</summary>
        [HttpPost]
        public IActionResult StartSession([FromBody] StartRoomSessionRequest request)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == request.RoomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Available")
                return BadRequest(new { message = $"Room is currently {room.Status}." });
            if (request.GuestCount < 1 || request.GuestCount > room.Capacity)
                return BadRequest(new { message = $"Guest count must be between 1 and {room.Capacity}." });

            var session = new RoomSessionDto
            {
                Id = _nextSessionId++,
                RoomId = room.Id,
                ClientName = request.ClientName,
                GuestCount = request.GuestCount,
                StartTime = DateTime.UtcNow,
                HourlyRate = room.HourlyRate,
                IsActive = true
            };

            _sessions.Add(session);

            room.Status = "Occupied";
            room.CurrentOccupancy = request.GuestCount;
            room.SessionStartTime = session.StartTime;
            room.SessionClientName = request.ClientName;
            room.ActiveSessionId = session.Id;

            return Ok(new
            {
                success = true,
                sessionId = session.Id,
                startTime = session.StartTime.ToString("O"), // ISO 8601 for JS
                clientName = request.ClientName,
                guestCount = request.GuestCount,
                hourlyRate = room.HourlyRate
            });
        }

        /// <summary>End an active room session → room becomes Available</summary>
        [HttpPost]
        public IActionResult EndSession(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId && s.IsActive);
            if (session == null) return NotFound(new { message = "Active session not found." });

            var room = _rooms.FirstOrDefault(r => r.Id == session.RoomId);
            if (room == null) return NotFound(new { message = "Room not found." });

            // Use local time to match DashboardController convention
            session.EndTime = DateTime.Now;
            session.IsActive = false;
            var duration = session.EndTime.Value - session.StartTime;
            session.TotalCost = Math.Round((decimal)duration.TotalHours * session.HourlyRate, 2);

            room.Status = "Available";
            room.CurrentOccupancy = 0;
            room.SessionStartTime = null;
            room.SessionClientName = null;
            room.ActiveSessionId = null;

            // ── Push into the shared completed-sessions list so Sessions page shows it ──
            DashboardController.AddCompletedSession(new SessionReportDto
            {
                Id = session.Id,
                DeviceName = room.Name,   // room name acts as "device" name
                DeviceType = "Room",
                CustomerName = session.ClientName,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Duration = duration,
                DurationFormatted = FormatDuration(duration),
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                Status = "Completed",
                PaymentMethod = "Cash"       // extend with real payment selection later
            });

            return Ok(new
            {
                success = true,
                clientName = session.ClientName,
                duration = FormatDuration(duration),
                totalCost = session.TotalCost,
                hourlyRate = session.HourlyRate
            });
        }

        /// <summary>Get live session info (for timer sync)</summary>
        [HttpGet]
        public IActionResult GetSession(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) return NotFound();

            var elapsed = (session.EndTime ?? DateTime.UtcNow) - session.StartTime;
            var cost = Math.Round((decimal)elapsed.TotalHours * session.HourlyRate, 2);

            return Json(new
            {
                id = session.Id,
                roomId = session.RoomId,
                clientName = session.ClientName,
                guestCount = session.GuestCount,
                startTime = session.StartTime.ToString("O"),
                hourlyRate = session.HourlyRate,
                currentCost = cost,
                isActive = session.IsActive
            });
        }

        // ─── Reservation Management ───────────────────────────────────────────

        /// <summary>Reserve a room → room becomes Reserved</summary>
        [HttpPost]
        public IActionResult Reserve([FromBody] CreateReservationRequest request)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == request.RoomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Available")
                return BadRequest(new { message = $"Room is currently {room.Status}." });

            var reservation = new RoomReservationDto
            {
                Id = _nextReservationId++,
                RoomId = room.Id,
                ClientName = request.ClientName,
                Phone = request.Phone,
                ReservationTime = request.ReservationTime,
                Notes = request.Notes
            };

            _reservations.Add(reservation);

            room.Status = "Reserved";
            room.ReservationClientName = request.ClientName;
            room.ReservationTime = request.ReservationTime;
            room.ReservationNotes = request.Notes;

            return Ok(new { success = true, reservationId = reservation.Id });
        }

        /// <summary>Cancel reservation → room becomes Available</summary>
        [HttpPost]
        public IActionResult CancelReservation(int roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Reserved")
                return BadRequest(new { message = "Room has no active reservation." });

            // Remove reservation record
            var reservation = _reservations.FirstOrDefault(r => r.RoomId == roomId);
            if (reservation != null) _reservations.Remove(reservation);

            room.Status = "Available";
            room.ReservationClientName = null;
            room.ReservationTime = null;
            room.ReservationNotes = null;

            return Ok(new { success = true });
        }

        /// <summary>Activate a reservation (check-in) → room becomes Occupied</summary>
        [HttpPost]
        public IActionResult ActivateReservation(int roomId)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == roomId);
            if (room == null) return NotFound(new { message = "Room not found." });
            if (room.Status != "Reserved")
                return BadRequest(new { message = "Room is not reserved." });

            var reservation = _reservations.FirstOrDefault(r => r.RoomId == roomId);
            if (reservation == null) return NotFound(new { message = "Reservation not found." });

            // Start a session for the reservation client
            var session = new RoomSessionDto
            {
                Id = _nextSessionId++,
                RoomId = room.Id,
                ClientName = reservation.ClientName,
                GuestCount = 1,
                StartTime = DateTime.UtcNow,
                HourlyRate = room.HourlyRate,
                IsActive = true
            };
            _sessions.Add(session);
            _reservations.Remove(reservation);

            room.Status = "Occupied";
            room.CurrentOccupancy = 1;
            room.SessionStartTime = session.StartTime;
            room.SessionClientName = reservation.ClientName;
            room.ActiveSessionId = session.Id;
            room.ReservationClientName = null;
            room.ReservationTime = null;
            room.ReservationNotes = null;

            return Ok(new
            {
                success = true,
                sessionId = session.Id,
                startTime = session.StartTime.ToString("O"),
                clientName = session.ClientName,
                hourlyRate = room.HourlyRate
            });
        }

        // ─── Card Partial & Receipt ───────────────────────────────────────────

        /// <summary>Returns a single room card partial — used by JS refreshRoomCard()</summary>
        [HttpGet]
        public IActionResult CardPartial(int id)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();
            return PartialView("_RoomCard", room);
        }

        /// <summary>Returns the session receipt partial — shown in receipt modal after EndSession</summary>
        [HttpGet]
        public IActionResult SessionReceipt(int sessionId)
        {
            var session = _sessions.FirstOrDefault(s => s.Id == sessionId);
            if (session == null) return NotFound();

            var room = _rooms.FirstOrDefault(r => r.Id == session.RoomId);
            var duration = (session.EndTime ?? DateTime.Now) - session.StartTime;

            var receipt = new SessionReceiptDto
            {
                SessionId = session.Id,
                DeviceName = room?.Name ?? "Room",
                CustomerName = session.ClientName,
                StartTime = session.StartTime,
                EndTime = session.EndTime ?? DateTime.Now,
                Duration = duration,
                DurationFormatted = FormatDuration(duration),
                HourlyRate = session.HourlyRate,
                TotalCost = session.TotalCost,
                PaymentMethod = "Cash",
                CompletedAt = session.EndTime ?? DateTime.Now,
            };

            // Use the full path so MVC finds it in /Views/Shared/ regardless of controller
            return PartialView("~/Views/Shared/_SessionReceipt.cshtml", receipt);
        }

        // ─── Helper ───────────────────────────────────────────────────────────
        private static string FormatDuration(TimeSpan ts)
            => $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }
}