using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Models.Entities;
using Serilog;
using System.Net;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/devices")]
[ApiController]
public class DevicesController : ControllerBase
{
    private readonly LprDbContext _context;

    public DevicesController(LprDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceResponse>>> GetDevices(string siteCode)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var devices = await _context.Devices
                .Where(d => d.SiteId == site.Id)
                .OrderBy(d => d.DeviceName)
                .ToListAsync();

            var response = devices.Select(d => new DeviceResponse
            {
                Id = d.Id,
                SerialNumber = d.SerialNumber,
                DeviceName = d.DeviceName,
                IpAddress = d.IpAddress?.ToString(),
                Port = d.Port,
                LocationDescription = d.LocationDescription,
                IsOnline = d.IsOnline,
                LastHeartbeat = d.LastHeartbeat,
                FirmwareVersion = d.FirmwareVersion,
                CreatedAt = d.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving devices for site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{deviceId:int}")]
    public async Task<ActionResult<DeviceResponse>> GetDevice(string siteCode, int deviceId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.Id == deviceId);

            if (device == null)
            {
                return NotFound($"Device '{deviceId}' not found in site '{siteCode}'");
            }

            var response = new DeviceResponse
            {
                Id = device.Id,
                SerialNumber = device.SerialNumber,
                DeviceName = device.DeviceName,
                IpAddress = device.IpAddress?.ToString(),
                Port = device.Port,
                LocationDescription = device.LocationDescription,
                IsOnline = device.IsOnline,
                LastHeartbeat = device.LastHeartbeat,
                FirmwareVersion = device.FirmwareVersion,
                CreatedAt = device.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving device {DeviceId} for site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<DeviceResponse>> CreateDevice(string siteCode, [FromBody] CreateDeviceRequest request)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Check if device with same serial number already exists in this site
            var existingDevice = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.SerialNumber == request.SerialNumber);

            if (existingDevice != null)
            {
                return Conflict($"Device with serial number '{request.SerialNumber}' already exists in site '{siteCode}'");
            }

            var device = new Device
            {
                SiteId = site.Id,
                SerialNumber = request.SerialNumber,
                DeviceName = request.DeviceName,
                IpAddress = !string.IsNullOrEmpty(request.IpAddress) && IPAddress.TryParse(request.IpAddress, out var ip) ? ip : null,
                Port = request.Port,
                LocationDescription = request.LocationDescription,
                IsOnline = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            var response = new DeviceResponse
            {
                Id = device.Id,
                SerialNumber = device.SerialNumber,
                DeviceName = device.DeviceName,
                IpAddress = device.IpAddress?.ToString(),
                Port = device.Port,
                LocationDescription = device.LocationDescription,
                IsOnline = device.IsOnline,
                LastHeartbeat = device.LastHeartbeat,
                FirmwareVersion = device.FirmwareVersion,
                CreatedAt = device.CreatedAt
            };

            Log.Information("Created new device: {SerialNumber} in site {SiteCode}", device.SerialNumber, siteCode);

            return CreatedAtAction(nameof(GetDevice), new { siteCode, deviceId = device.Id }, response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating device {SerialNumber} for site {SiteCode}", request.SerialNumber, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPut("{deviceId:int}")]
    public async Task<ActionResult<DeviceResponse>> UpdateDevice(string siteCode, int deviceId, [FromBody] UpdateDeviceRequest request)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.Id == deviceId);

            if (device == null)
            {
                return NotFound($"Device '{deviceId}' not found in site '{siteCode}'");
            }

            device.DeviceName = request.DeviceName;
            device.IpAddress = !string.IsNullOrEmpty(request.IpAddress) && IPAddress.TryParse(request.IpAddress, out var ip) ? ip : null;
            device.Port = request.Port;
            device.LocationDescription = request.LocationDescription;
            device.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new DeviceResponse
            {
                Id = device.Id,
                SerialNumber = device.SerialNumber,
                DeviceName = device.DeviceName,
                IpAddress = device.IpAddress?.ToString(),
                Port = device.Port,
                LocationDescription = device.LocationDescription,
                IsOnline = device.IsOnline,
                LastHeartbeat = device.LastHeartbeat,
                FirmwareVersion = device.FirmwareVersion,
                CreatedAt = device.CreatedAt
            };

            Log.Information("Updated device: {SerialNumber} in site {SiteCode}", device.SerialNumber, siteCode);

            return Ok(response);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating device {DeviceId} for site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpDelete("{deviceId:int}")]
    public async Task<IActionResult> DeleteDevice(string siteCode, int deviceId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var device = await _context.Devices
                .Include(d => d.PlateRecognitionResults)
                .Include(d => d.EntryLogs)
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.Id == deviceId);

            if (device == null)
            {
                return NotFound($"Device '{deviceId}' not found in site '{siteCode}'");
            }

            // Check if device has associated data
            if (device.PlateRecognitionResults.Any() || device.EntryLogs.Any())
            {
                return BadRequest("Cannot delete device with associated recognition results or entry logs. Please archive the device instead.");
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            Log.Information("Deleted device: {SerialNumber} from site {SiteCode}", device.SerialNumber, siteCode);

            return NoContent();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting device {DeviceId} for site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{deviceId:int}/status")]
    public async Task<ActionResult<object>> GetDeviceStatus(string siteCode, int deviceId)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.Id == deviceId);

            if (device == null)
            {
                return NotFound($"Device '{deviceId}' not found in site '{siteCode}'");
            }

            var today = DateTime.Today;
            var lastHeartbeat = await _context.DeviceHeartbeats
                .Where(h => h.DeviceId == deviceId)
                .OrderByDescending(h => h.ReceivedAt)
                .FirstOrDefaultAsync();

            var todayRecognitions = await _context.PlateRecognitionResults
                .CountAsync(p => p.DeviceId == deviceId && p.CreatedAt >= today);

            var status = new
            {
                DeviceId = device.Id,
                SerialNumber = device.SerialNumber,
                DeviceName = device.DeviceName,
                IsOnline = device.IsOnline,
                LastHeartbeat = device.LastHeartbeat,
                LastHeartbeatType = lastHeartbeat?.HeartbeatType,
                FirmwareVersion = device.FirmwareVersion,
                TodayRecognitions = todayRecognitions,
                UptimeMinutes = device.LastHeartbeat.HasValue ?
                    (int?)(int)(DateTime.UtcNow - device.LastHeartbeat.Value).TotalMinutes : null
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error retrieving status for device {DeviceId} in site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpPost("{deviceId:int}/commands")]
    public async Task<IActionResult> SendCommand(string siteCode, int deviceId, [FromBody] object command)
    {
        try
        {
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == site.Id && d.Id == deviceId);

            if (device == null)
            {
                return NotFound($"Device '{deviceId}' not found in site '{siteCode}'");
            }

            // Queue command for device
            var commandQueue = new CommandQueue
            {
                SiteId = site.Id,
                DeviceId = deviceId,
                CommandType = "manual_command",
                CommandData = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(command)),
                Priority = 1,
                IsProcessed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.CommandQueue.Add(commandQueue);
            await _context.SaveChangesAsync();

            Log.Information("Queued command for device {SerialNumber} in site {SiteCode}", device.SerialNumber, siteCode);

            return Ok(new { message = "Command queued successfully", commandId = commandQueue.Id });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error sending command to device {DeviceId} in site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
