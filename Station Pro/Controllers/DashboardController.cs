using Microsoft.AspNetCore.Mvc;

namespace Station_Pro.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using StationPro.Application.DTOs;
    using StationPro.Domain.Entities;

    namespace Station_Pro.Controllers
    {
        public class DashboardController : Controller
        {
            // Main Dashboard Index
            public IActionResult Index()
            {
                // Create test data for dashboard stats
                var stats = new DashboardStatsDto
                {
                    TodayRevenue = 2450.50m,
                    TotalSessions = 23,
                    ActiveDevices = 8,
                    TotalDevices = 12,
                    ActiveSessions = 5,
                    AverageSessionCost = 106.54m
                };

                return View(stats);
            }

            // Live Stats (for HTMX auto-refresh)
            public IActionResult LiveStats()
            {
                var stats = new DashboardStatsDto
                {
                    TodayRevenue = 2450.50m,
                    TotalSessions = 23,
                    ActiveDevices = 8,
                    TotalDevices = 12,
                    ActiveSessions = 5,
                    AverageSessionCost = 106.54m
                };

                return PartialView("_DashboardStats", stats);
            }

            // Active Sessions (for HTMX auto-refresh)
            public IActionResult ActiveSessions()
            {
                // Create test active sessions with realistic Egyptian names
                var sessions = new List<SessionDto>
            {
                new SessionDto
                {
                    Id = 1,
                    DeviceId = 1,
                    DeviceName = "PS5 - Station 1",
                    CustomerName = "Ahmed Khaled",
                    CustomerPhone = "01012345678",
                    StartTime = DateTime.Now.AddHours(-2).AddMinutes(-45),
                    HourlyRate = 50.00m,
                    TotalCost = 137.50m,
                    Duration = TimeSpan.FromHours(2).Add(TimeSpan.FromMinutes(45)),
                    Status = "Active"
                },
                new SessionDto
                {
                    Id = 2,
                    DeviceId = 4,
                    DeviceName = "PS4 - Station 2",
                    CustomerName = "Mohamed Hassan",
                    CustomerPhone = "01123456789",
                    StartTime = DateTime.Now.AddHours(-1).AddMinutes(-30),
                    HourlyRate = 40.00m,
                    TotalCost = 60.00m,
                    Duration = TimeSpan.FromHours(1).Add(TimeSpan.FromMinutes(30)),
                    Status = "Active"
                },
                new SessionDto
                {
                    Id = 3,
                    DeviceId = 5,
                    DeviceName = "Xbox Series X",
                    CustomerName = null, // Guest
                    CustomerPhone = null,
                    StartTime = DateTime.Now.AddMinutes(-55),
                    HourlyRate = 45.00m,
                    TotalCost = 41.25m,
                    Duration = TimeSpan.FromMinutes(55),
                    Status = "Active"
                },
                new SessionDto
                {
                    Id = 4,
                    DeviceId = 7,
                    DeviceName = "Gaming PC - Ultimate",
                    CustomerName = "Youssef Mahmoud",
                    CustomerPhone = "01234567890",
                    StartTime = DateTime.Now.AddMinutes(-40),
                    HourlyRate = 60.00m,
                    TotalCost = 40.00m,
                    Duration = TimeSpan.FromMinutes(40),
                    Status = "Active"
                },
                new SessionDto
                {
                    Id = 5,
                    DeviceId = 2,
                    DeviceName = "PS5 - Station 2",
                    CustomerName = "Omar Ali",
                    CustomerPhone = "01098765432",
                    StartTime = DateTime.Now.AddMinutes(-20),
                    HourlyRate = 50.00m,
                    TotalCost = 16.67m,
                    Duration = TimeSpan.FromMinutes(20),
                    Status = "Active"
                }
            };

                return PartialView("_ActiveSessions", sessions);
            }

            // Device Cards (for HTMX auto-refresh)
            public IActionResult DeviceCards()
            {
                // Create test devices
                var devices = new List<DeviceDto>
            {
                // In-Use Devices
                new DeviceDto
                {
                    Id = 1,
                    Name = "PS5 - Station 1",
                    Type = DeviceType.PS5,
                    HourlyRate = 50.00m,
                    IsAvailable = false,
                    Status = "In Use",
                    CurrentSession = new SessionDto
                    {
                        Id = 1,
                        CustomerName = "Ahmed Khaled",
                        TotalCost = 137.50m
                    }
                },
                new DeviceDto
                {
                    Id = 2,
                    Name = "PS5 - Station 2",
                    Type = DeviceType.PS5,
                    HourlyRate = 50.00m,
                    IsAvailable = false,
                    Status = "In Use",
                    CurrentSession = new SessionDto
                    {
                        Id = 5,
                        CustomerName = "Omar Ali",
                        TotalCost = 16.67m
                    }
                },
                new DeviceDto
                {
                    Id = 4,
                    Name = "PS4 - Station 2",
                    Type = DeviceType.PS4,
                    HourlyRate = 40.00m,
                    IsAvailable = false,
                    Status = "In Use",
                    CurrentSession = new SessionDto
                    {
                        Id = 2,
                        CustomerName = "Mohamed Hassan",
                        TotalCost = 60.00m
                    }
                },
                new DeviceDto
                {
                    Id = 5,
                    Name = "Xbox Series X",
                    Type = DeviceType.Xbox,
                    HourlyRate = 45.00m,
                    IsAvailable = false,
                    Status = "In Use",
                    CurrentSession = new SessionDto
                    {
                        Id = 3,
                        CustomerName = null, // Guest
                        TotalCost = 41.25m
                    }
                },
                new DeviceDto
                {
                    Id = 7,
                    Name = "Gaming PC - Ultimate",
                    Type = DeviceType.PC,
                    HourlyRate = 60.00m,
                    IsAvailable = false,
                    Status = "In Use",
                    CurrentSession = new SessionDto
                    {
                        Id = 4,
                        CustomerName = "Youssef Mahmoud",
                        TotalCost = 40.00m
                    }
                },
                
                // Available Devices
                new DeviceDto
                {
                    Id = 3,
                    Name = "PS4 - Station 1",
                    Type = DeviceType.PS4,
                    HourlyRate = 40.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                },
                new DeviceDto
                {
                    Id = 6,
                    Name = "Xbox One",
                    Type = DeviceType.Xbox,
                    HourlyRate = 35.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                },
                new DeviceDto
                {
                    Id = 8,
                    Name = "Gaming PC - Standard",
                    Type = DeviceType.PC,
                    HourlyRate = 55.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                },
                new DeviceDto
                {
                    Id = 9,
                    Name = "PS5 - Station 3",
                    Type = DeviceType.PS5,
                    HourlyRate = 50.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                },
                new DeviceDto
                {
                    Id = 10,
                    Name = "PS4 - Station 3",
                    Type = DeviceType.PS4,
                    HourlyRate = 40.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                },
                new DeviceDto
                {
                    Id = 11,
                    Name = "VR Station",
                    Type = DeviceType.PC,
                    HourlyRate = 70.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                },
                new DeviceDto
                {
                    Id = 12,
                    Name = "Retro Gaming Corner",
                    Type = DeviceType.Xbox,
                    HourlyRate = 30.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                }
            };

                return PartialView("_DeviceCards", devices);
            }

            // Timer Update (for HTMX session timer)
            public IActionResult Timer(int sessionId)
            {
                // In a real app, you'd fetch the actual session and calculate duration
                // For now, return a random timer for demo purposes
                var random = new Random(sessionId); // Use sessionId as seed for consistency
                var hours = random.Next(0, 5);
                var minutes = random.Next(0, 60);
                var seconds = random.Next(0, 60);

                return Content($"{hours:00}:{minutes:00}:{seconds:00}");
            }

        }
    }
}
