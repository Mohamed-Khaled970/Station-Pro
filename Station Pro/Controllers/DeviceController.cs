// StationPro.Web/Controllers/DeviceController.cs
// TEMPORARY VERSION - Uses dummy data for UI testing
// Replace with real service implementation later

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
        // Return all dummy devices
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
        // Create new device with dummy data
        var newDevice = new DeviceDto
        {
            Id = _devices.Max(d => d.Id) + 1,
            Name = dto.Name,
            Type = dto.Type,
            HourlyRate = dto.HourlyRate,
            IsAvailable = true,
            Status = "Available",
            CurrentSession = null
        };

        _devices.Add(newDevice);

        // Return partial view for HTMX
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

        // Update device (Status is string in DTO)
        device.Name = dto.Name;
        device.HourlyRate = dto.HourlyRate;
        device.IsAvailable = dto.IsActive;
        device.Status = dto.Status.ToString(); // Convert enum to string

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
    // GENERATE DUMMY DATA
    // ============================================

    private static List<DeviceDto> GenerateDummyDevices()
    {
        return new List<DeviceDto>
        {
            // Available PS5 Devices
            new DeviceDto
            {
                Id = 1,
                Name = "PS5 - 1",
                Type = DeviceType.PS5,
                HourlyRate = 50,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            new DeviceDto
            {
                Id = 2,
                Name = "PS5 - 2",
                Type = DeviceType.PS5,
                HourlyRate = 50,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // In Use PS5
            new DeviceDto
            {
                Id = 3,
                Name = "PS5 - 3",
                Type = DeviceType.PS5,
                HourlyRate = 50,
                IsAvailable = false,
                Status = "In Use",
                CurrentSession = new SessionDto
                {
                    Id = 1,
                    DeviceId = 3,
                    DeviceName = "PS5 - 3",
                    CustomerName = "Ahmed Mohamed",
                    StartTime = DateTime.Now.AddMinutes(-45),
                    HourlyRate = 50,
                    TotalCost = 37.50m,
                    Status = "Active",
                    Duration = TimeSpan.FromMinutes(45)
                }
            },
            
            // Available PS4 Devices
            new DeviceDto
            {
                Id = 4,
                Name = "PS4 - 1",
                Type = DeviceType.PS4,
                HourlyRate = 30,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            new DeviceDto
            {
                Id = 5,
                Name = "PS4 - 2",
                Type = DeviceType.PS4,
                HourlyRate = 30,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // In Use PS4
            new DeviceDto
            {
                Id = 6,
                Name = "PS4 - 3",
                Type = DeviceType.PS4,
                HourlyRate = 30,
                IsAvailable = false,
                Status = "In Use",
                CurrentSession = new SessionDto
                {
                    Id = 2,
                    DeviceId = 6,
                    DeviceName = "PS4 - 3",
                    CustomerName = "Ali Hassan",
                    StartTime = DateTime.Now.AddMinutes(-120),
                    HourlyRate = 30,
                    TotalCost = 60m,
                    Status = "Active",
                    Duration = TimeSpan.FromMinutes(120)
                }
            },
            
            // Xbox Available
            new DeviceDto
            {
                Id = 7,
                Name = "Xbox One",
                Type = DeviceType.Xbox,
                HourlyRate = 35,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // PC Gaming
            new DeviceDto
            {
                Id = 8,
                Name = "Gaming PC - 1",
                Type = DeviceType.PC,
                HourlyRate = 40,
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
                HourlyRate = 30,
                IsAvailable = false,
                Status = "Maintenance",
                CurrentSession = null
            },
            
            // Ping Pong Table
            new DeviceDto
            {
                Id = 10,
                Name = "Ping Pong Table",
                Type = DeviceType.PingPong,
                HourlyRate = 20,
                IsAvailable = true,
                Status = "Available",
                CurrentSession = null
            },
            
            // Pool Table - In Use
            new DeviceDto
            {
                Id = 11,
                Name = "Pool Table",
                Type = DeviceType.Pool,
                HourlyRate = 25,
                IsAvailable = false,
                Status = "In Use",
                CurrentSession = new SessionDto
                {
                    Id = 3,
                    DeviceId = 11,
                    DeviceName = "Pool Table",
                    CustomerName = null, // Guest
                    StartTime = DateTime.Now.AddMinutes(-30),
                    HourlyRate = 25,
                    TotalCost = 12.50m,
                    Status = "Active",
                    Duration = TimeSpan.FromMinutes(30)
                }
            },
            
            // Offline Device
            new DeviceDto
            {
                Id = 12,
                Name = "PS3 - 1",
                Type = DeviceType.PS3,
                HourlyRate = 20,
                IsAvailable = false,
                Status = "Offline",
                CurrentSession = null
            }
        };
    }
}