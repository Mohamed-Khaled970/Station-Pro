using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Domain.Entities;

namespace Station_Pro.Controllers.Station_Pro.Controllers
{
    public class DashboardController : Controller
    {
        // Static sessions list to track active sessions
        private static List<SessionDto> _activeSessions = GenerateInitialSessions();

        // NEW: Static list to store completed sessions
        private static List<SessionReportDto> _completedSessions = new List<SessionReportDto>();

        // Main Dashboard Index
        public IActionResult Index()
        {
            var stats = new DashboardStatsDto
            {
                TodayRevenue = CalculateTodayRevenue(),
                TotalSessions = _activeSessions.Count + _completedSessions.Count,
                ActiveDevices = _activeSessions.Count,
                TotalDevices = 12,
                ActiveSessions = _activeSessions.Count,
                AverageSessionCost = 106.54m
            };

            return View(stats);
        }

        // Live Stats (for HTMX auto-refresh)
        public IActionResult LiveStats()
        {
            var stats = new DashboardStatsDto
            {
                TodayRevenue = CalculateTodayRevenue(),
                TotalSessions = _activeSessions.Count + _completedSessions.Count,
                ActiveDevices = _activeSessions.Count,
                TotalDevices = 12,
                ActiveSessions = _activeSessions.Count,
                AverageSessionCost = CalculateAverageCost()
            };

            return PartialView("_DashboardStats", stats);
        }

        // Active Sessions (for HTMX auto-refresh)
        public IActionResult ActiveSessions()
        {
            // Update durations and costs for all active sessions
            foreach (var session in _activeSessions)
            {
                var elapsed = DateTime.Now - session.StartTime;
                session.Duration = elapsed;
                session.TotalCost = (decimal)(elapsed.TotalHours * (double)session.HourlyRate);
            }

            return PartialView("_ActiveSessions", _activeSessions);
        }

        // Device Cards (for HTMX auto-refresh)
        public IActionResult DeviceCards()
        {
            var devices = new List<DeviceDto>();

            // Add devices that are in use
            foreach (var session in _activeSessions)
            {
                devices.Add(new DeviceDto
                {
                    Id = session.DeviceId,
                    Name = session.DeviceName,
                    Type = GetDeviceType(session.DeviceName),
                    HourlyRate = session.HourlyRate,
                    IsAvailable = false,
                    Status = "In Use",
                    CurrentSession = session
                });
            }

            // Add available devices
            var usedDeviceIds = _activeSessions.Select(s => s.DeviceId).ToList();
            if (!usedDeviceIds.Contains(3))
            {
                devices.Add(new DeviceDto
                {
                    Id = 3,
                    Name = "PS4 - Station 1",
                    Type = DeviceType.PS4,
                    HourlyRate = 40.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                });
            }
            if (!usedDeviceIds.Contains(6))
            {
                devices.Add(new DeviceDto
                {
                    Id = 6,
                    Name = "Xbox One",
                    Type = DeviceType.Xbox,
                    HourlyRate = 35.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                });
            }
            if (!usedDeviceIds.Contains(8))
            {
                devices.Add(new DeviceDto
                {
                    Id = 8,
                    Name = "Gaming PC - Standard",
                    Type = DeviceType.PC,
                    HourlyRate = 55.00m,
                    IsAvailable = true,
                    Status = "Available",
                    CurrentSession = null
                });
            }

            // Add more available devices...
            for (int i = 9; i <= 12; i++)
            {
                if (!usedDeviceIds.Contains(i))
                {
                    devices.Add(new DeviceDto
                    {
                        Id = i,
                        Name = $"Device {i}",
                        Type = DeviceType.PS4,
                        HourlyRate = 40.00m,
                        IsAvailable = true,
                        Status = "Available",
                        CurrentSession = null
                    });
                }
            }

            return PartialView("_DeviceCards", devices);
        }

        // ============================================
        // END SESSION - Main endpoint (UPDATED)
        // ============================================

        [HttpPost]
        public IActionResult End(int sessionId, int paymentMethod = 1)
        {
            var session = _activeSessions.FirstOrDefault(s => s.Id == sessionId);

            if (session == null)
            {
                return NotFound(new { success = false, message = "Session not found" });
            }

            // Calculate final values
            session.EndTime = DateTime.Now;
            var duration = session.EndTime.Value - session.StartTime;
            var totalCost = (decimal)(duration.TotalHours * (double)session.HourlyRate);

            // Create receipt data
            var receipt = new SessionReceiptDto
            {
                SessionId = session.Id,
                DeviceName = session.DeviceName,
                CustomerName = session.CustomerName ?? "Guest",
                StartTime = session.StartTime,
                EndTime = session.EndTime.Value,
                Duration = duration,
                DurationFormatted = $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}",
                HourlyRate = session.HourlyRate,
                TotalCost = totalCost,
                PaymentMethod = paymentMethod == 1 ? "Cash" : "Card",
                CompletedAt = DateTime.Now
            };

            // NEW: Save completed session to the completed sessions list
            var completedSession = new SessionReportDto
            {
                Id = session.Id,
                DeviceName = session.DeviceName,
                DeviceType = GetDeviceType(session.DeviceName).ToString(),
                CustomerName = session.CustomerName,
                StartTime = session.StartTime,
                EndTime = session.EndTime.Value,
                Duration = duration,
                DurationFormatted = receipt.DurationFormatted,
                HourlyRate = session.HourlyRate,
                TotalCost = totalCost,
                Status = "Completed",
                PaymentMethod = paymentMethod == 1 ? "Cash" : "Card"
            };

            _completedSessions.Add(completedSession);

            // Remove from active sessions
            _activeSessions.Remove(session);

            // Return the receipt partial view for display in modal
            return PartialView("_SessionReceipt", receipt);
        }

        // NEW: Method to get completed sessions (for ReportController)
        public static List<SessionReportDto> GetCompletedSessions()
        {
            return _completedSessions;
        }

        public static List<SessionDto> GetActiveSessions()
        {
            return _activeSessions;
        }

        // ============================================
        // HELPER METHODS
        // ============================================

        private static List<SessionDto> GenerateInitialSessions()
        {
            return new List<SessionDto>
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
                    Status = "Active"
                },
                new SessionDto
                {
                    Id = 3,
                    DeviceId = 5,
                    DeviceName = "Xbox Series X",
                    CustomerName = null,
                    CustomerPhone = null,
                    StartTime = DateTime.Now.AddMinutes(-55),
                    HourlyRate = 45.00m,
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
                    Status = "Active"
                }
            };
        }

        private decimal CalculateTodayRevenue()
        {
            // Calculate current active sessions value
            decimal activeRevenue = 0;
            foreach (var session in _activeSessions)
            {
                var duration = DateTime.Now - session.StartTime;
                activeRevenue += (decimal)(duration.TotalHours * (double)session.HourlyRate);
            }

            // Add completed sessions revenue
            decimal completedRevenue = _completedSessions
                .Where(s => s.StartTime.Date == DateTime.Today)
                .Sum(s => s.TotalCost);

            return activeRevenue + completedRevenue;
        }

        private decimal CalculateAverageCost()
        {
            if (_activeSessions.Count == 0) return 0;

            decimal totalCost = 0;
            foreach (var session in _activeSessions)
            {
                var duration = DateTime.Now - session.StartTime;
                totalCost += (decimal)(duration.TotalHours * (double)session.HourlyRate);
            }

            return totalCost / _activeSessions.Count;
        }

        private DeviceType GetDeviceType(string deviceName)
        {
            if (deviceName.Contains("PS5")) return DeviceType.PS5;
            if (deviceName.Contains("PS4")) return DeviceType.PS4;
            if (deviceName.Contains("Xbox")) return DeviceType.Xbox;
            if (deviceName.Contains("PC")) return DeviceType.PC;
            return DeviceType.Other;
        }
    }
}