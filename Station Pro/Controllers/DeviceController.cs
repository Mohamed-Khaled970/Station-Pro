// StationPro.Web/Controllers/DeviceController.cs
// UPDATED VERSION with Single/Multi Session Support

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Domain.Entities;

namespace StationPro.Web.Controllers;

public class DeviceController : Controller
{
    // Static list to simulate database (temporary)
    private static List<DeviceDto> _devices = GenerateDummyDevices();

    // ============================================
    // INDEX - List All Devices
    // ============================================

    [HttpGet]
    public IActionResult Index()
    {
        return View(_devices);
    }

    // ============================================
    // GET - Get Single Device (for Edit Modal)
    // ============================================

    [HttpGet("Device/Get/{id}")]
    public IActionResult Get(int id)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);

        if (device == null)
        {
            return NotFound(new { message = "Device not found" });
        }

        return Ok(device);
    }

    // ============================================
    // CREATE - Add New Device
    // ============================================

    [HttpPost]
    public IActionResult Create([FromForm] CreateDeviceDto dto)
    {
        // Validate multi-session rate if enabled
        if (dto.SupportsMultiSession && !dto.MultiSessionRate.HasValue)
        {
            return BadRequest(new { message = "Multi-session rate is required when multi-session is enabled" });
        }

        var newDevice = new DeviceDto
        {
            Id = _devices.Any() ? _devices.Max(d => d.Id) + 1 : 1,
            Name = dto.Name,
            Type = dto.Type,
            SingleSessionRate = dto.SingleSessionRate,
            MultiSessionRate = dto.MultiSessionRate,
            SupportsMultiSession = dto.SupportsMultiSession,
            IsAvailable = true,
            Status = "Available",
            CurrentSession = null
        };

        _devices.Add(newDevice);

        return PartialView("_DeviceCard", newDevice);
    }

    // ============================================
    // UPDATE - Edit Existing Device
    // ============================================

    [HttpPut("Device/Update/{id}")]
    public IActionResult Update(int id, [FromBody] UpdateDeviceDto dto)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);

        if (device == null)
        {
            return NotFound(new { message = "Device not found" });
        }

        // Validate multi-session
        if (dto.SupportsMultiSession && !dto.MultiSessionRate.HasValue)
        {
            return BadRequest(new { message = "Multi-session rate is required when multi-session is enabled" });
        }

        device.Name = dto.Name;
        device.SingleSessionRate = dto.SingleSessionRate;
        device.MultiSessionRate = dto.MultiSessionRate;
        device.SupportsMultiSession = dto.SupportsMultiSession;
        device.IsAvailable = dto.IsActive;
        device.Status = dto.Status.ToString();

        return Ok(new { message = "Device updated successfully" });
    }

    // ============================================
    // DELETE - Remove Device
    // ============================================

    [HttpDelete("Device/Delete/{id}")]
    public IActionResult Delete(int id)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);

        if (device == null)
        {
            return NotFound(new { message = "Device not found" });
        }

        _devices.Remove(device);

        return Ok(new { message = "Device deleted successfully" });
    }

    // ============================================
    // GENERATE DUMMY DATA with Single/Multi Rates
    // ============================================

    private static List<DeviceDto> GenerateDummyDevices()
    {
        return new List<DeviceDto>
        {
            // PS5 Devices with Multi-Session Support
            new DeviceDto
            {
                Id = 1,
                Name = "PS5 - 1",
                Type = DeviceType.PS5,
                SingleSessionRate = 50,
                MultiSessionRate = 80,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            new DeviceDto
            {
                Id = 2,
                Name = "PS5 - 2",
                Type = DeviceType.PS5,
                SingleSessionRate = 50,
                MultiSessionRate = 80,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // In Use PS5 with Multi-Session
            new DeviceDto
            {
                Id = 3,
                Name = "PS5 - 3",
                Type = DeviceType.PS5,
                SingleSessionRate = 50,
                MultiSessionRate = 80,
                SupportsMultiSession = true,
                IsAvailable = false,
                Status = "In Use",
                CurrentSession = new SessionDto
                {
                    Id = 1,
                    DeviceId = 3,
                    DeviceName = "PS5 - 3",
                    CustomerName = "Ahmed Mohamed",
                    StartTime = DateTime.Now.AddMinutes(-45),
                    HourlyRate = 80, // Using multi-session rate
                    TotalCost = 60m,
                    Status = "Active",
                    Duration = TimeSpan.FromMinutes(45)
                }
            },
            
            // PS4 Devices with Multi-Session
            new DeviceDto
            {
                Id = 4,
                Name = "PS4 - 1",
                Type = DeviceType.PS4,
                SingleSessionRate = 30,
                MultiSessionRate = 50,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            new DeviceDto
            {
                Id = 5,
                Name = "PS4 - 2",
                Type = DeviceType.PS4,
                SingleSessionRate = 30,
                MultiSessionRate = 50,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // In Use PS4 with Single Session
            new DeviceDto
            {
                Id = 6,
                Name = "PS4 - 3",
                Type = DeviceType.PS4,
                SingleSessionRate = 30,
                MultiSessionRate = 50,
                SupportsMultiSession = true,
                IsAvailable = false,
                Status = "In Use",
                CurrentSession = new SessionDto
                {
                    Id = 2,
                    DeviceId = 6,
                    DeviceName = "PS4 - 3",
                    CustomerName = "Ali Hassan",
                    StartTime = DateTime.Now.AddMinutes(-120),
                    HourlyRate = 30, // Using single session rate
                    TotalCost = 60m,
                    Status = "Active",
                    Duration = TimeSpan.FromMinutes(120)
                }
            },
            
            // Xbox with Multi-Session
            new DeviceDto
            {
                Id = 7,
                Name = "Xbox One",
                Type = DeviceType.Xbox,
                SingleSessionRate = 35,
                MultiSessionRate = 60,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // PC Gaming (Single Session Only)
            new DeviceDto
            {
                Id = 8,
                Name = "Gaming PC - 1",
                Type = DeviceType.PC,
                SingleSessionRate = 40,
                MultiSessionRate = null,
                SupportsMultiSession = false,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // Maintenance Device
            new DeviceDto
            {
                Id = 9,
                Name = "PS4 - 4",
                Type = DeviceType.PS4,
                SingleSessionRate = 30,
                MultiSessionRate = 50,
                SupportsMultiSession = true,
                IsAvailable = false,
                Status = "Maintenance",
                CurrentSession = null
            },
            
            // Ping Pong with Multi-Session
            new DeviceDto
            {
                Id = 10,
                Name = "Ping Pong Table",
                Type = DeviceType.PingPong,
                SingleSessionRate = 20,
                MultiSessionRate = 35,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // Pool Table with Multi-Session - In Use
            new DeviceDto
            {
                Id = 11,
                Name = "Pool Table",
                Type = DeviceType.Pool,
                SingleSessionRate = 25,
                MultiSessionRate = 40,
                SupportsMultiSession = true,
                IsAvailable = false,
                Status = "In Use",
                CurrentSession = new SessionDto
                {
                    Id = 3,
                    DeviceId = 11,
                    DeviceName = "Pool Table",
                    CustomerName = null,
                    StartTime = DateTime.Now.AddMinutes(-30),
                    HourlyRate = 40, // Using multi-session rate
                    TotalCost = 20m,
                    Status = "Active",
                    Duration = TimeSpan.FromMinutes(30)
                }
            },
            
            // Billiards with Multi-Session
            new DeviceDto
            {
                Id = 12,
                Name = "Billiards Table",
                Type = DeviceType.Billiards,
                SingleSessionRate = 30,
                MultiSessionRate = 50,
                SupportsMultiSession = true,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            }
        };
    }
}