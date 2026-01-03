using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;

namespace Station_Pro.Controllers
{
    public class RoomController : Controller
    {
        // Dummy data storage
        private static List<RoomDto> _rooms = new List<RoomDto>
        {
            new RoomDto
            {
                Id = 1,
                Name = "VIP Room 1",
                HasAC = true,
                HourlyRate = 100.00m,
                Capacity = 4,
                IsActive = true,
                Status = "Available",
                CurrentOccupancy = 0,
                DeviceCount = 2
            },
            new RoomDto
            {
                Id = 2,
                Name = "VIP Room 2",
                HasAC = true,
                HourlyRate = 120.00m,
                Capacity = 6,
                IsActive = true,
                Status = "Occupied",
                CurrentOccupancy = 4,
                DeviceCount = 3
            },
            new RoomDto
            {
                Id = 3,
                Name = "Standard Room A",
                HasAC = false,
                HourlyRate = 60.00m,
                Capacity = 3,
                IsActive = true,
                Status = "Available",
                CurrentOccupancy = 0,
                DeviceCount = 1
            },
            new RoomDto
            {
                Id = 4,
                Name = "Standard Room B",
                HasAC = false,
                HourlyRate = 60.00m,
                Capacity = 3,
                IsActive = true,
                Status = "Available",
                CurrentOccupancy = 0,
                DeviceCount = 1
            },
            new RoomDto
            {
                Id = 5,
                Name = "Premium Lounge",
                HasAC = true,
                HourlyRate = 150.00m,
                Capacity = 8,
                IsActive = true,
                Status = "Occupied",
                CurrentOccupancy = 6,
                DeviceCount = 4
            },
            new RoomDto
            {
                Id = 6,
                Name = "Party Room",
                HasAC = true,
                HourlyRate = 200.00m,
                Capacity = 10,
                IsActive = true,
                Status = "Reserved",
                CurrentOccupancy = 0,
                DeviceCount = 5
            },
            new RoomDto
            {
                Id = 7,
                Name = "Gaming Pod 1",
                HasAC = false,
                HourlyRate = 40.00m,
                Capacity = 2,
                IsActive = true,
                Status = "Available",
                CurrentOccupancy = 0,
                DeviceCount = 1
            },
            new RoomDto
            {
                Id = 8,
                Name = "Conference Room",
                HasAC = true,
                HourlyRate = 80.00m,
                Capacity = 12,
                IsActive = false,
                Status = "Maintenance",
                CurrentOccupancy = 0,
                DeviceCount = 2
            }
        };

        public IActionResult Index()
        {
            return View(_rooms);
        }

        [HttpPost]
        public IActionResult Create(RoomDto room)
        {
            room.Id = _rooms.Max(r => r.Id) + 1;
            room.Status = "Available";
            _rooms.Add(room);

            return PartialView("_RoomCard", room);
        }

        [HttpGet]
        public IActionResult Get(int id)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
                return NotFound();

            return Json(room);
        }

        [HttpPut]
        public IActionResult Update(int id, [FromBody] RoomDto updatedRoom)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
                return NotFound();

            room.Name = updatedRoom.Name;
            room.HasAC = updatedRoom.HasAC;
            room.HourlyRate = updatedRoom.HourlyRate;
            room.Capacity = updatedRoom.Capacity;
            room.IsActive = updatedRoom.IsActive;

            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == id);
            if (room == null)
                return NotFound();

            _rooms.Remove(room);
            return Ok();
        }
    }
}
