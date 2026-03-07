// StationPro.Web/Controllers/DeviceController.cs
// UPDATED VERSION with Single/Multi Session Support

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Application.Interfaces;
using StationPro.Domain.Entities;
using StationPro.Filters;

namespace StationPro.Web.Controllers;

[SubscriptionRequired]
public class DeviceController : Controller
{
    private readonly ISessionService _sessions;

    public DeviceController(ISessionService sessions)
    {
        _sessions = sessions;
    }

    // ─── View ─────────────────────────────────────────────────────────────

    public IActionResult Index()
        => View(DeviceStore.GetAll());

    // ─── Device CRUD ──────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Get(int id)
    {
        var device = DeviceStore.GetById(id);
        return device == null ? NotFound() : Json(device);
    }

    [HttpPost]
    public IActionResult Create([FromBody] DeviceDto device)
    {
        var created = DeviceStore.Add(device);
        return Ok(created);
    }

    [HttpPut]
    public IActionResult Update(int id, [FromBody] DeviceDto updated)
    {
        var device = DeviceStore.GetById(id);
        if (device == null) return NotFound();

        device.Name = updated.Name;
        device.Type = updated.Type;
        device.SingleSessionRate = updated.SingleSessionRate;
        device.MultiSessionRate = updated.MultiSessionRate;
        device.SupportsMultiSession = updated.SupportsMultiSession;
        device.IsActive = updated.IsActive;

        DeviceStore.Update(device);
        return Ok();
    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var device = DeviceStore.GetById(id);
        if (device == null) return NotFound();

        if (!device.IsAvailable)
            return BadRequest(new { message = "Cannot delete a device that is currently in use." });

        DeviceStore.Delete(id);
        return Ok();
    }

    // ─── Session: Start ───────────────────────────────────────────────────

    [HttpPost]
    public IActionResult StartSession([FromBody] StartDeviceSessionRequest request)
    {
        var device = DeviceStore.GetById(request.DeviceId);
        if (device == null)
            return NotFound(new { success = false, message = "Device not found." });

        if (!device.IsAvailable)
            return BadRequest(new { success = false, message = "Device is not available." });

        try
        {
            var result = _sessions.StartDeviceSession(request, device);
            DeviceStore.Update(device);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ─── Session: End ─────────────────────────────────────────────────────

    [HttpPost]
    public IActionResult EndSession(int sessionId, int paymentMethod = 1)
    {
        var session = _sessions.GetById(sessionId);
        if (session == null || !session.IsActive)
            return NotFound(new { message = "Active session not found." });

        var device = DeviceStore.GetById(session.DeviceId!.Value);
        if (device == null) return NotFound(new { message = "Device not found." });

        try
        {
            var result = _sessions.EndDeviceSession(sessionId, paymentMethod, device);
            DeviceStore.Update(device);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── Card partial refresh ─────────────────────────────────────────────

    [HttpGet]
    public IActionResult CardPartial(int id)
    {
        var device = DeviceStore.GetById(id);
        return device == null ? NotFound() : PartialView("_DeviceCard", device);
    }
}