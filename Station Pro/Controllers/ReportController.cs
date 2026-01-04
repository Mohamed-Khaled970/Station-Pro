// StationPro.Web/Controllers/ReportController.cs
// UPDATED VERSION - Uses real completed sessions from DashboardController

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Domain.Entities;
using System.Text;
using Station_Pro.Controllers.Station_Pro.Controllers;

namespace StationPro.Web.Controllers;

public class ReportController : Controller
{
    // ============================================
    // INDEX - Main Reports Page
    // ============================================

    [HttpGet]
    public IActionResult Index(string period = "today")
    {
        var report = GenerateReport(period);
        ViewBag.SelectedPeriod = period;
        return View(report);
    }

    // ============================================
    // EXPORT TO CSV
    // ============================================

    [HttpGet]
    public IActionResult ExportCsv(string period = "today")
    {
        var report = GenerateReport(period);

        var csv = new StringBuilder();
        csv.AppendLine("Date,Device,Customer,Duration,Cost,Status,Payment");

        foreach (var session in report.Sessions)
        {
            csv.AppendLine($"{session.StartTime:yyyy-MM-dd HH:mm}," +
                          $"{session.DeviceName}," +
                          $"{session.CustomerName ?? "Guest"}," +
                          $"{session.DurationFormatted}," +
                          $"{session.TotalCost}," +
                          $"{session.Status}," +
                          $"{session.PaymentMethod}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"report-{period}-{DateTime.Now:yyyyMMdd}.csv");
    }

    // ============================================
    // PRINT REPORT
    // ============================================

    [HttpGet]
    public IActionResult Print(string period = "today")
    {
        var report = GenerateReport(period);
        ViewBag.SelectedPeriod = period;
        return View("PrintReport", report);
    }

    // ============================================
    // GENERATE REPORT DATA (UPDATED)
    // ============================================

    private CompleteReportDto GenerateReport(string period)
    {
        var (startDate, endDate) = GetDateRange(period);

        // Get real completed sessions from DashboardController
        var completedSessions = DashboardController.GetCompletedSessions();

        // Filter sessions by date range
        var sessions = completedSessions
            .Where(s => s.StartTime >= startDate && s.StartTime <= endDate)
            .OrderByDescending(s => s.StartTime)
            .ToList();

        // If no real sessions, generate some dummy data for demonstration
        if (!sessions.Any())
        {
            sessions = GenerateDummySessions(startDate, endDate);
        }

        return new CompleteReportDto
        {
            Summary = GenerateSummary(sessions, startDate, endDate),
            Sessions = sessions,
            DailyRevenue = GenerateDailyRevenue(sessions),
            DevicePerformance = GenerateDevicePerformance(sessions),
            HourlyUsage = GenerateHourlyUsage(sessions)
        };
    }

    private (DateTime start, DateTime end) GetDateRange(string period)
    {
        var now = DateTime.Now;

        return period.ToLower() switch
        {
            "today" => (now.Date, now.Date.AddDays(1).AddSeconds(-1)),
            "yesterday" => (now.Date.AddDays(-1), now.Date.AddSeconds(-1)),
            "week" => (now.Date.AddDays(-7), now),
            "month" => (now.Date.AddDays(-30), now),
            _ => (now.Date, now)
        };
    }

    private List<SessionReportDto> GenerateDummySessions(DateTime start, DateTime end)
    {
        var random = new Random();
        var sessions = new List<SessionReportDto>();
        var deviceNames = new[] { "PS5 - 1", "PS5 - 2", "PS4 - 1", "PS4 - 2", "Xbox One", "Pool Table" };
        var customerNames = new[] { "Ahmed Mohamed", "Ali Hassan", "Mohamed Ahmed", "Omar Ibrahim", null, "Sarah Ali", null };

        var totalDays = (end - start).Days + 1;
        var sessionsPerDay = 8; // Average 8 sessions per day

        for (int day = 0; day < totalDays; day++)
        {
            var currentDate = start.AddDays(day);
            var numSessions = random.Next(5, sessionsPerDay + 3);

            for (int i = 0; i < numSessions; i++)
            {
                var deviceName = deviceNames[random.Next(deviceNames.Length)];
                var hourlyRate = deviceName.Contains("PS5") ? 50m :
                                deviceName.Contains("PS4") ? 30m :
                                deviceName.Contains("Xbox") ? 35m : 25m;

                var startTime = currentDate.AddHours(random.Next(10, 22));
                var durationMinutes = random.Next(30, 180);
                var endTime = startTime.AddMinutes(durationMinutes);
                var duration = TimeSpan.FromMinutes(durationMinutes);

                var cost = (decimal)(duration.TotalHours) * hourlyRate;

                sessions.Add(new SessionReportDto
                {
                    Id = sessions.Count + 1,
                    DeviceName = deviceName,
                    DeviceType = deviceName.Contains("PS") ? "PlayStation" :
                                deviceName.Contains("Xbox") ? "Xbox" : "Pool",
                    CustomerName = customerNames[random.Next(customerNames.Length)],
                    StartTime = startTime,
                    EndTime = endTime,
                    Duration = duration,
                    DurationFormatted = $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}:{duration.Seconds:D2}",
                    HourlyRate = hourlyRate,
                    TotalCost = Math.Round(cost, 2),
                    Status = random.Next(100) < 95 ? "Completed" : "Cancelled",
                    PaymentMethod = random.Next(100) < 80 ? "Cash" : random.Next(100) < 50 ? "Card" : "Online"
                });
            }
        }

        return sessions.OrderByDescending(s => s.StartTime).ToList();
    }

    private ReportSummaryDto GenerateSummary(List<SessionReportDto> sessions, DateTime start, DateTime end)
    {
        var completed = sessions.Where(s => s.Status == "Completed").ToList();

        return new ReportSummaryDto
        {
            StartDate = start,
            EndDate = end,
            TotalSessions = sessions.Count,
            CompletedSessions = completed.Count,
            CancelledSessions = sessions.Count - completed.Count,
            TotalRevenue = completed.Sum(s => s.TotalCost),
            AverageSessionRevenue = completed.Any() ? completed.Average(s => s.TotalCost) : 0,
            TotalDuration = TimeSpan.FromMinutes(completed.Sum(s => s.Duration.TotalMinutes)),
            AverageDuration = completed.Any() ?
                TimeSpan.FromMinutes(completed.Average(s => s.Duration.TotalMinutes)) : TimeSpan.Zero,
            MostUsedDevice = completed.GroupBy(s => s.DeviceName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A",
            MostPopularTime = GetMostPopularTime(completed)
        };
    }

    private string GetMostPopularTime(List<SessionReportDto> sessions)
    {
        if (!sessions.Any()) return "N/A";

        var mostPopularHour = sessions
            .GroupBy(s => s.StartTime.Hour)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? 14;

        var startHour = mostPopularHour % 12 == 0 ? 12 : mostPopularHour % 12;
        var endHour = (mostPopularHour + 1) % 12 == 0 ? 12 : (mostPopularHour + 1) % 12;
        var startPeriod = mostPopularHour < 12 ? "AM" : "PM";
        var endPeriod = mostPopularHour + 1 < 12 ? "AM" : "PM";

        return $"{startHour} {startPeriod} - {endHour} {endPeriod}";
    }

    private List<DailyRevenueDto> GenerateDailyRevenue(List<SessionReportDto> sessions)
    {
        return sessions
            .Where(s => s.Status == "Completed")
            .GroupBy(s => s.StartTime.Date)
            .Select(g => new DailyRevenueDto
            {
                Date = g.Key,
                DateFormatted = g.Key.ToString("MMM dd, yyyy"),
                SessionCount = g.Count(),
                Revenue = g.Sum(s => s.TotalCost),
                DayOfWeek = g.Key.ToString("dddd")
            })
            .OrderBy(d => d.Date)
            .ToList();
    }

    private List<DevicePerformanceDto> GenerateDevicePerformance(List<SessionReportDto> sessions)
    {
        var completed = sessions.Where(s => s.Status == "Completed").ToList();

        if (!completed.Any()) return new List<DevicePerformanceDto>();

        return completed
            .GroupBy(s => new { s.DeviceName, s.DeviceType })
            .Select(g => new DevicePerformanceDto
            {
                DeviceName = g.Key.DeviceName,
                DeviceType = g.Key.DeviceType,
                SessionCount = g.Count(),
                TotalUsageTime = TimeSpan.FromMinutes(g.Sum(s => s.Duration.TotalMinutes)),
                TotalRevenue = g.Sum(s => s.TotalCost),
                UtilizationPercentage = Math.Round(g.Count() * 100.0 / completed.Count, 1),
                AverageSessionCost = g.Average(s => s.TotalCost)
            })
            .OrderByDescending(d => d.TotalRevenue)
            .ToList();
    }

    private List<HourlyUsageDto> GenerateHourlyUsage(List<SessionReportDto> sessions)
    {
        var completed = sessions.Where(s => s.Status == "Completed").ToList();

        if (!completed.Any()) return new List<HourlyUsageDto>();

        return completed
            .GroupBy(s => s.StartTime.Hour)
            .Select(g => new HourlyUsageDto
            {
                Hour = g.Key,
                TimeRange = $"{(g.Key % 12 == 0 ? 12 : g.Key % 12)} {(g.Key < 12 ? "AM" : "PM")} - " +
                           $"{((g.Key + 1) % 12 == 0 ? 12 : (g.Key + 1) % 12)} {(g.Key + 1 < 12 ? "AM" : "PM")}",
                SessionCount = g.Count(),
                Revenue = g.Sum(s => s.TotalCost)
            })
            .OrderBy(h => h.Hour)
            .ToList();
    }
}