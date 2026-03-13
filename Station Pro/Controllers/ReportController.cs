// StationPro.Web/Controllers/ReportController.cs

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Application.Enums;
using StationPro.Domain.Entities;
using StationPro.Infrastructure.Helpers;
using System.Text;
using Station_Pro.Controllers.Station_Pro.Controllers;
using StationPro.Filters;
using StationPro.Application.Contracts.Services;

namespace StationPro.Web.Controllers;

[SubscriptionRequired]
public class ReportController : Controller
{
    private readonly ISessionService _sessions;

    public ReportController(ISessionService sessions)
    {
        _sessions = sessions;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string period = "today")
    {
        var report = await GenerateReportAsync(period);
        ViewBag.SelectedPeriod = period;
        return View(report);
    }

    [HttpGet]
    public async Task<IActionResult> ExportCsv(string period = "today")
    {
        var report = await GenerateReportAsync(period);

        var csv = new StringBuilder();
        csv.AppendLine("Date,Device,Customer,Duration,Cost,Status,Payment");

        foreach (var s in report.Sessions)
        {
            csv.AppendLine(
                $"{s.StartTime:yyyy-MM-dd HH:mm}," +
                $"{s.DeviceName}," +
                $"{s.CustomerName ?? "Guest"}," +
                $"{s.DurationFormatted}," +
                $"{s.TotalCost}," +
                $"{s.Status}," +
                $"{s.PaymentMethod}");
        }

        var bytes = Encoding.UTF8.GetBytes(csv.ToString());
        return File(bytes, "text/csv", $"report-{period}-{DateTime.Now:yyyyMMdd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> Print(string period = "today")
    {
        var report = await GenerateReportAsync(period);
        ViewBag.SelectedPeriod = period;
        return View("PrintReport", report);
    }

    // ── Report generation ─────────────────────────────────────────────────

    private async Task<CompleteReportDto> GenerateReportAsync(string period)
    {
        // FIX: Use Egypt-local date boundaries instead of UTC.
        // Old code: GetDateRange used DateTime.Now.Date (UTC on server),
        // which meant "today" started at midnight UTC = 2:00 AM Egypt time.
        // Sessions before 2 AM Egypt would fall into server's "yesterday".
        var (startUtc, endUtc) = TimeZoneHelper.GetUtcDateRange(period);

        var filter = new SessionFilterRequest
        {
            DateFilter = "all",   // we filter by exact UTC range below
            Status = "completed",
            Page = 1,
            PageSize = 10_000
        };

        var pageResult = await _sessions.GetPageAsync(filter);

        var sessions = pageResult.Sessions
            .Where(s => s.StartTime >= startUtc && s.StartTime <= endUtc)
            .OrderByDescending(s => s.StartTime)
            .Select(MapToReportDto)
            .ToList();

        return new CompleteReportDto
        {
            Summary = GenerateSummary(sessions, startUtc, endUtc),
            Sessions = sessions,
            DailyRevenue = GenerateDailyRevenue(sessions),
            DevicePerformance = GenerateDevicePerformance(sessions),
            HourlyUsage = GenerateHourlyUsage(sessions)
        };
    }

    // ── Mapping ───────────────────────────────────────────────────────────

    private static SessionReportDto MapToReportDto(UnifiedSessionDto s) => new()
    {
        Id = s.Id,
        DeviceName = s.SourceName,
        DeviceType = s.SourceCategory,
        CustomerName = s.CustomerName,
        StartTime = s.StartTime,
        EndTime = s.EndTime,
        Duration = s.Duration,
        DurationFormatted = s.DurationFormatted,
        HourlyRate = s.HourlyRate,
        TotalCost = s.TotalCost,
        Status = s.StatusString,
        PaymentMethod = s.PaymentMethod?.ToString() ?? "Cash",
        SessionType = s.SessionTypeString
    };

    // ── Helpers ───────────────────────────────────────────────────────────

    private static ReportSummaryDto GenerateSummary(
        List<SessionReportDto> sessions,
        DateTime start,
        DateTime end)
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
            AverageDuration = completed.Any()
                ? TimeSpan.FromMinutes(completed.Average(s => s.Duration.TotalMinutes))
                : TimeSpan.Zero,
            MostUsedDevice = completed
                .GroupBy(s => s.DeviceName)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A",
            MostPopularTime = GetMostPopularTime(completed)
        };
    }

    private static string GetMostPopularTime(List<SessionReportDto> sessions)
    {
        if (!sessions.Any()) return "N/A";

        var h = sessions.GroupBy(s => s.StartTime.Hour)
                             .OrderByDescending(g => g.Count())
                             .FirstOrDefault()?.Key ?? 14;
        var startH = h % 12 == 0 ? 12 : h % 12;
        var endH = (h + 1) % 12 == 0 ? 12 : (h + 1) % 12;
        var startAp = h < 12 ? "AM" : "PM";
        var endAp = h + 1 < 12 ? "AM" : "PM";

        return $"{startH} {startAp} - {endH} {endAp}";
    }

    private static List<DailyRevenueDto> GenerateDailyRevenue(
        List<SessionReportDto> sessions) =>
        sessions
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

    private static List<DevicePerformanceDto> GenerateDevicePerformance(
        List<SessionReportDto> sessions)
    {
        var completed = sessions.Where(s => s.Status == "Completed").ToList();
        if (!completed.Any()) return new();

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

    private static List<HourlyUsageDto> GenerateHourlyUsage(
        List<SessionReportDto> sessions)
    {
        var completed = sessions.Where(s => s.Status == "Completed").ToList();
        if (!completed.Any()) return new();

        return completed
            .GroupBy(s => s.StartTime.Hour)
            .Select(g => new HourlyUsageDto
            {
                Hour = g.Key,
                TimeRange = $"{(g.Key % 12 == 0 ? 12 : g.Key % 12)} " +
                            $"{(g.Key < 12 ? "AM" : "PM")} - " +
                            $"{((g.Key + 1) % 12 == 0 ? 12 : (g.Key + 1) % 12)} " +
                            $"{(g.Key + 1 < 12 ? "AM" : "PM")}",
                SessionCount = g.Count(),
                Revenue = g.Sum(s => s.TotalCost)
            })
            .OrderBy(h => h.Hour)
            .ToList();
    }
}