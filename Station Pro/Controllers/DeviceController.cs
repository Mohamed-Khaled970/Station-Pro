// StationPro.Web/Controllers/DeviceController.cs
// UPDATED VERSION with Single/Multi Session Support

using Microsoft.AspNetCore.Mvc;
using StationPro.Application.DTOs;
using StationPro.Application.Interfaces.InMemory;
using StationPro.Application.Interfaces;
using StationPro.Domain.Entities;
using StationPro.Filters;
using StationPro.Application.Contracts.Services;

namespace StationPro.Web.Controllers;

[SubscriptionRequired]
public class DeviceController : Controller
{
    private readonly IDeviceService _devices;
    private readonly Application.Contracts.Services.ISessionService _sessions;

    public DeviceController(IDeviceService devices, Application.Contracts.Services.ISessionService sessions)
    {
        _devices = devices;
        _sessions = sessions;
    }

    // ── View ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        var devices = await _devices.GetAllAsync();
        return View(devices);
    }

    // ── Device CRUD ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var device = await _devices.GetByIdAsync(id);
        return device == null ? NotFound() : Json(device);
    }

    /// <summary>
    /// Called by the "Add Device" HTMX form.
    /// Returns the new _DeviceCard partial so HTMX can swap it in.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateDeviceDto dto)
    {
        try
        {
            var created = await _devices.CreateAsync(dto);
            return PartialView("_DeviceCard", created);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Called by the "Edit Device" JS form (PUT via fetch).
    /// Accepts a JSON body matching UpdateDeviceDto.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateDeviceDto dto)
    {
        try
        {
            var updated = await _devices.UpdateAsync(id, dto);
            return Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _devices.DeleteAsync(id);
            return Ok(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ── Session: Start ────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> StartSession([FromBody] StartDeviceSessionRequest request)
    {
        try
        {
            var result = await _sessions.StartDeviceSessionAsync(request, request.DeviceId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ── Session: End ──────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> EndSession(int sessionId, int paymentMethod = 1)
    {
        try
        {
            var result = await _sessions.EndDeviceSessionAsync(sessionId, paymentMethod);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ── Card partial refresh ───────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CardPartial(int id)
    {
        var device = await _devices.GetByIdAsync(id);
        return device == null ? NotFound() : PartialView("_DeviceCard", device);
    }
}