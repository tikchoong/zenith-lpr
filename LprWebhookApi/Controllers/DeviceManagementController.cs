using LprWebhookApi.Data;
using LprWebhookApi.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LprWebhookApi.Controllers;

[Route("api/device-management")]
[ApiController]
public class DeviceManagementController : ControllerBase
{
    private readonly LprDbContext _context;
    private readonly ILogger<DeviceManagementController> _logger;

    public DeviceManagementController(LprDbContext context, ILogger<DeviceManagementController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Enable or disable whitelist sync for a device
    /// </summary>
    [HttpPost("{deviceId}/whitelist-sync")]
    public async Task<IActionResult> SetWhitelistSync(int deviceId, [FromBody] SetFeatureRequest request)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return NotFound($"Device with ID {deviceId} not found");
        }

        device.WhitelistStartSync = request.Enabled;
        if (request.Enabled)
        {
            device.WhitelistSyncStatus = null; // Reset status when enabling
            device.WhitelistSyncBatchesSent = 0;
            device.WhitelistSyncTotalBatches = 0;
            device.WhitelistSyncStartedAt = null;
        }
        device.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Whitelist sync {Action} for device {DeviceId}",
            request.Enabled ? "enabled" : "disabled", deviceId);

        return Ok(new
        {
            message = $"Whitelist sync {(request.Enabled ? "enabled" : "disabled")}",
            deviceId,
            enabled = request.Enabled
        });
    }

    /// <summary>
    /// Enable or disable screenshot capture for a device
    /// </summary>
    [HttpPost("{deviceId}/screenshot-capture")]
    public async Task<IActionResult> SetScreenshotCapture(int deviceId, [FromBody] SetFeatureRequest request)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return NotFound($"Device with ID {deviceId} not found");
        }

        device.CaptureScreenshotEnabled = request.Enabled;
        if (!request.Enabled)
        {
            device.ScreenshotCaptureStatus = null; // Clear status when disabling
        }
        device.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Screenshot capture {Action} for device {DeviceId}",
            request.Enabled ? "enabled" : "disabled", deviceId);

        return Ok(new
        {
            message = $"Screenshot capture {(request.Enabled ? "enabled" : "disabled")}",
            deviceId,
            enabled = request.Enabled
        });
    }

    /// <summary>
    /// Enable or disable whitelist sync for all devices in a site
    /// </summary>
    [HttpPost("sites/{siteCode}/whitelist-sync")]
    public async Task<IActionResult> SetWhitelistSyncForSite(string siteCode, [FromBody] SetFeatureRequest request)
    {
        var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
        if (site == null)
        {
            return NotFound($"Site with code '{siteCode}' not found");
        }

        // Use traditional approach for complex conditional updates
        var devices = await _context.Devices
            .Where(d => d.SiteId == site.Id)
            .ToListAsync();

        foreach (var device in devices)
        {
            device.WhitelistStartSync = request.Enabled;
            if (request.Enabled)
            {
                device.WhitelistSyncStatus = null;
                device.WhitelistSyncBatchesSent = 0;
                device.WhitelistSyncTotalBatches = 0;
                device.WhitelistSyncStartedAt = null;
            }
            device.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        var devicesUpdated = devices.Count;

        _logger.LogInformation("Whitelist sync {Action} for {DeviceCount} devices in site {SiteCode}",
            request.Enabled ? "enabled" : "disabled", devicesUpdated, siteCode);

        return Ok(new
        {
            message = $"Whitelist sync {(request.Enabled ? "enabled" : "disabled")} for {devicesUpdated} devices in site {siteCode}",
            siteCode,
            devicesUpdated,
            enabled = request.Enabled
        });
    }

    /// <summary>
    /// Enable or disable screenshot capture for all devices in a site
    /// </summary>
    [HttpPost("sites/{siteCode}/screenshot-capture")]
    public async Task<IActionResult> SetScreenshotCaptureForSite(string siteCode, [FromBody] SetFeatureRequest request)
    {
        var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
        if (site == null)
        {
            return NotFound($"Site with code '{siteCode}' not found");
        }

        var devices = await _context.Devices
            .Where(d => d.SiteId == site.Id)
            .ToListAsync();

        foreach (var device in devices)
        {
            device.CaptureScreenshotEnabled = request.Enabled;
            if (!request.Enabled)
            {
                device.ScreenshotCaptureStatus = null;
            }
            device.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        var devicesUpdated = devices.Count;

        _logger.LogInformation("Screenshot capture {Action} for {DeviceCount} devices in site {SiteCode}",
            request.Enabled ? "enabled" : "disabled", devicesUpdated, siteCode);

        return Ok(new
        {
            message = $"Screenshot capture {(request.Enabled ? "enabled" : "disabled")} for {devicesUpdated} devices in site {siteCode}",
            siteCode,
            devicesUpdated,
            enabled = request.Enabled
        });
    }

    /// <summary>
    /// Get device status including all feature states
    /// </summary>
    [HttpGet("{deviceId}/status")]
    public async Task<ActionResult<DeviceStatusResponse>> GetDeviceStatus(int deviceId)
    {
        var device = await _context.Devices
            .Include(d => d.Site)
            .FirstOrDefaultAsync(d => d.Id == deviceId);

        if (device == null)
        {
            return NotFound($"Device with ID {deviceId} not found");
        }

        return Ok(new DeviceStatusResponse
        {
            DeviceId = device.Id,
            SerialNumber = device.SerialNumber,
            DeviceName = device.DeviceName ?? "",
            SiteCode = device.Site.SiteCode,
            IsOnline = device.IsOnline,
            LastHeartbeat = device.LastHeartbeat,
            WhitelistSync = new WhitelistSyncStatus
            {
                Enabled = device.WhitelistStartSync,
                Status = device.WhitelistSyncStatus,
                LastStarted = device.WhitelistSyncStartedAt,
                BatchesSent = device.WhitelistSyncBatchesSent,
                TotalBatches = device.WhitelistSyncTotalBatches
            },
            ScreenshotCapture = new ScreenshotCaptureStatus
            {
                Enabled = device.CaptureScreenshotEnabled,
                Status = device.ScreenshotCaptureStatus,
                LastRequest = device.LastScreenshotRequest
            }
        });
    }

    /// <summary>
    /// Get status for all devices in a site
    /// </summary>
    [HttpGet("sites/{siteCode}/status")]
    public async Task<ActionResult<List<DeviceStatusResponse>>> GetSiteDevicesStatus(string siteCode)
    {
        var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
        if (site == null)
        {
            return NotFound($"Site with code '{siteCode}' not found");
        }

        var devices = await _context.Devices
            .Include(d => d.Site)
            .Where(d => d.SiteId == site.Id)
            .ToListAsync();

        var deviceStatuses = devices.Select(device => new DeviceStatusResponse
        {
            DeviceId = device.Id,
            SerialNumber = device.SerialNumber,
            DeviceName = device.DeviceName ?? "",
            SiteCode = device.Site.SiteCode,
            IsOnline = device.IsOnline,
            LastHeartbeat = device.LastHeartbeat,
            WhitelistSync = new WhitelistSyncStatus
            {
                Enabled = device.WhitelistStartSync,
                Status = device.WhitelistSyncStatus,
                LastStarted = device.WhitelistSyncStartedAt,
                BatchesSent = device.WhitelistSyncBatchesSent,
                TotalBatches = device.WhitelistSyncTotalBatches
            },
            ScreenshotCapture = new ScreenshotCaptureStatus
            {
                Enabled = device.CaptureScreenshotEnabled,
                Status = device.ScreenshotCaptureStatus,
                LastRequest = device.LastScreenshotRequest
            }
        }).ToList();

        return Ok(deviceStatuses);
    }
}
