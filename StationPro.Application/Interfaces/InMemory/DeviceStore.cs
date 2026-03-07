using StationPro.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StationPro.Domain.Entities;
namespace StationPro.Application.Interfaces.InMemory
{
    /// <summary>
    /// Central in-memory store for devices.
    /// </summary>
    public static class DeviceStore
    {
        private static int _nextId = 10;

        private static readonly List<DeviceDto> _devices = new()
        {
            new() { Id=1, Name="PS5 - Station 1", Type=DeviceType.PS5,  SingleSessionRate=50m, MultiSessionRate=80m,  SupportsMultiSession=true,  IsActive=true },
            new() { Id=2, Name="PS5 - Station 2", Type=DeviceType.PS5,  SingleSessionRate=50m, MultiSessionRate=80m,  SupportsMultiSession=true,  IsActive=true },
            new() { Id=3, Name="PS4 - Station 1", Type=DeviceType.PS4,  SingleSessionRate=40m, MultiSessionRate=65m,  SupportsMultiSession=true,  IsActive=true },
            new() { Id=4, Name="PS4 - Station 2", Type=DeviceType.PS4,  SingleSessionRate=40m, MultiSessionRate=65m,  SupportsMultiSession=true,  IsActive=true },
            new() { Id=5, Name="Xbox Series X",   Type=DeviceType.Xbox, SingleSessionRate=45m, MultiSessionRate=72m,  SupportsMultiSession=true,  IsActive=true },
            new() { Id=6, Name="Gaming PC - Ultimate", Type=DeviceType.PC, SingleSessionRate=60m,                     SupportsMultiSession=false, IsActive=true },
            new() { Id=7, Name="Gaming PC - Standard", Type=DeviceType.PC, SingleSessionRate=45m,                     SupportsMultiSession=false, IsActive=true },
            new() { Id=8, Name="Ping Pong Table", Type=DeviceType.PingPong, SingleSessionRate=30m, MultiSessionRate=30m, SupportsMultiSession=true, IsActive=true },
            new() { Id=9, Name="Billiards Table", Type=DeviceType.Billiards, SingleSessionRate=35m, MultiSessionRate=35m, SupportsMultiSession=true, IsActive=true },
        };

        public static List<DeviceDto> GetAll() { lock (_devices) return _devices.ToList(); }
        public static List<DeviceDto> GetAvailable() { lock (_devices) return _devices.Where(d => d.IsAvailable).ToList(); }

        public static DeviceDto? GetById(int id) { lock (_devices) return _devices.FirstOrDefault(d => d.Id == id); }

        public static DeviceDto Add(DeviceDto device)
        {
            lock (_devices)
            {
                device.Id = _nextId++;
                _devices.Add(device);
                return device;
            }
        }

        public static bool Update(DeviceDto updated)
        {
            lock (_devices)
            {
                int idx = _devices.FindIndex(d => d.Id == updated.Id);
                if (idx < 0) return false;
                _devices[idx] = updated;
                return true;
            }
        }

        public static bool Delete(int id)
        {
            lock (_devices)
            {
                var d = _devices.FirstOrDefault(d => d.Id == id);
                if (d == null) return false;
                _devices.Remove(d);
                return true;
            }
        }
    }
}
