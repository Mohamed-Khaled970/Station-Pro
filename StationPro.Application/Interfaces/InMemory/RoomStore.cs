using StationPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StationPro.Application.Interfaces.InMemory
{

    /// <summary>
    /// Central in-memory store for rooms and reservations.
    /// Replace with EF Core repository calls when DB is ready.
    /// </summary>
    public static class RoomStore
    {
        private static int _nextRoomId = 9;
        private static int _nextReservationId = 2;

        private static readonly List<RoomDto> _rooms = new()
        {
            new() { Id=1, Name="VIP Room 1",      HasAC=true,  SingleHourlyRate=100m, MultiHourlyRate=160m, Capacity=4,  IsActive=true,  Status="Available",    DeviceCount=2 },
            new() { Id=2, Name="VIP Room 2",      HasAC=true,  SingleHourlyRate=120m, MultiHourlyRate=190m, Capacity=6,  IsActive=true,  Status="Occupied",     DeviceCount=3,
                    SessionStartTime=DateTime.UtcNow.AddMinutes(-47), SessionClientName="Ahmed Ali",
                    ActiveSessionId=1, SessionType="Multi",  HourlyRate=190m, CurrentOccupancy=4 },
            new() { Id=3, Name="Standard Room A", HasAC=false, SingleHourlyRate= 60m, MultiHourlyRate=100m, Capacity=3,  IsActive=true,  Status="Available",    DeviceCount=1 },
            new() { Id=4, Name="Standard Room B", HasAC=false, SingleHourlyRate= 60m, MultiHourlyRate=100m, Capacity=3,  IsActive=true,  Status="Available",    DeviceCount=1 },
            new() { Id=5, Name="Premium Lounge",  HasAC=true,  SingleHourlyRate=150m, MultiHourlyRate=240m, Capacity=8,  IsActive=true,  Status="Occupied",     DeviceCount=4,
                    SessionStartTime=DateTime.UtcNow.AddMinutes(-23), SessionClientName="Omar Hassan",
                    ActiveSessionId=2, SessionType="Single", HourlyRate=150m, CurrentOccupancy=2 },
            new() { Id=6, Name="Party Room",      HasAC=true,  SingleHourlyRate=200m, MultiHourlyRate=320m, Capacity=10, IsActive=true,  Status="Reserved",     DeviceCount=5,
                    ReservationClientName="Sara Mohamed", ReservationTime=DateTime.UtcNow.AddHours(2), ReservationNotes="Birthday party" },
            new() { Id=7, Name="Gaming Pod 1",    HasAC=false, SingleHourlyRate= 40m, MultiHourlyRate= 65m, Capacity=2,  IsActive=true,  Status="Available",    DeviceCount=1 },
            new() { Id=8, Name="Conference Room", HasAC=true,  SingleHourlyRate= 80m, MultiHourlyRate=130m, Capacity=12, IsActive=false, Status="Maintenance",  DeviceCount=2 },
        };

        private static readonly List<RoomReservationDto> _reservations = new()
        {
            new() { Id=1, RoomId=6, ClientName="Sara Mohamed", Phone="01012345678",
                    ReservationTime=DateTime.UtcNow.AddHours(2), Notes="Birthday party" },
        };

        // ── Room CRUD ─────────────────────────────────────────────────────────

        public static List<RoomDto> GetAll() { lock (_rooms) return _rooms.ToList(); }

        public static RoomDto? GetById(int id) { lock (_rooms) return _rooms.FirstOrDefault(r => r.Id == id); }

        public static RoomDto Add(RoomDto room)
        {
            lock (_rooms)
            {
                room.Id = _nextRoomId++;
                room.Status = "Available";
                _rooms.Add(room);
                return room;
            }
        }

        public static bool Update(RoomDto updated)
        {
            lock (_rooms)
            {
                int idx = _rooms.FindIndex(r => r.Id == updated.Id);
                if (idx < 0) return false;
                _rooms[idx] = updated;
                return true;
            }
        }

        public static bool Delete(int id)
        {
            lock (_rooms)
            {
                var room = _rooms.FirstOrDefault(r => r.Id == id);
                if (room == null) return false;
                _rooms.Remove(room);
                return true;
            }
        }

        public static string GetName(int id) =>
            GetById(id)?.Name ?? $"Room {id}";

        // ── Reservations ──────────────────────────────────────────────────────

        public static RoomReservationDto? GetReservation(int roomId)
        {
            lock (_reservations) return _reservations.FirstOrDefault(r => r.RoomId == roomId);
        }

        public static RoomReservationDto AddReservation(RoomReservationDto res)
        {
            lock (_reservations)
            {
                res.Id = _nextReservationId++;
                _reservations.Add(res);
                return res;
            }
        }

        public static void RemoveReservation(int roomId)
        {
            lock (_reservations)
            {
                var res = _reservations.FirstOrDefault(r => r.RoomId == roomId);
                if (res != null) _reservations.Remove(res);
            }
        }
    }
}
