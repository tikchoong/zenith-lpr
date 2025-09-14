using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LprWebhookApi.Data;
using LprWebhookApi.Services;
using Serilog;

namespace LprWebhookApi.Controllers;

[Route("api/lpr/sites/{siteCode}/devices")]
[ApiController]
public class WhitelistSyncController : ControllerBase
{
    private readonly LprDbContext _context;
    private readonly WhitelistSyncService _whitelistSyncService;

    public WhitelistSyncController(LprDbContext context, WhitelistSyncService whitelistSyncService)
    {
        _context = context;
        _whitelistSyncService = whitelistSyncService;
    }

    /// <summary>
    /// Test endpoint to verify controller is working
    /// </summary>
    [HttpGet("test")]
    public IActionResult Test()
    {
        return Ok(new { message = "WhitelistSyncController is working!", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Trigger whitelist sync for a specific device
    /// </summary>
    [HttpPost("{deviceId}/sync-whitelist")]
    public async Task<IActionResult> TriggerWhitelistSync(string siteCode, int deviceId)
    {
        try
        {
            // Verify site exists
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Verify device exists and belongs to site
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.SiteId == site.Id);
            if (device == null)
            {
                return NotFound($"Device {deviceId} not found in site '{siteCode}'");
            }

            var success = await _whitelistSyncService.TriggerSync(deviceId);
            if (success)
            {
                Log.Information("Whitelist sync triggered for device {DeviceId} in site {SiteCode}", deviceId, siteCode);
                return Ok(new { message = "Whitelist sync triggered successfully", deviceId, siteCode });
            }
            else
            {
                return BadRequest(new { error = "Failed to trigger whitelist sync" });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error triggering whitelist sync for device {DeviceId} in site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get whitelist sync status for a specific device
    /// </summary>
    [HttpGet("{deviceId}/sync-status")]
    public async Task<IActionResult> GetWhitelistSyncStatus(string siteCode, int deviceId)
    {
        try
        {
            // Verify site exists
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Verify device exists and belongs to site
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.SiteId == site.Id);
            if (device == null)
            {
                return NotFound($"Device {deviceId} not found in site '{siteCode}'");
            }

            var status = await _whitelistSyncService.GetSyncStatus(deviceId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting whitelist sync status for device {DeviceId} in site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel ongoing whitelist sync for a specific device
    /// </summary>
    [HttpPost("{deviceId}/cancel-sync")]
    public async Task<IActionResult> CancelWhitelistSync(string siteCode, int deviceId)
    {
        try
        {
            // Verify site exists
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            // Verify device exists and belongs to site
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.Id == deviceId && d.SiteId == site.Id);
            if (device == null)
            {
                return NotFound($"Device {deviceId} not found in site '{siteCode}'");
            }

            var success = await _whitelistSyncService.CancelSync(deviceId);
            if (success)
            {
                Log.Information("Whitelist sync cancelled for device {DeviceId} in site {SiteCode}", deviceId, siteCode);
                return Ok(new { message = "Whitelist sync cancelled successfully", deviceId, siteCode });
            }
            else
            {
                return BadRequest(new { error = "No active sync to cancel or device not found" });
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error cancelling whitelist sync for device {DeviceId} in site {SiteCode}", deviceId, siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    /// Get whitelist sync status for all devices in a site
    /// </summary>
    [HttpGet("sync-status")]
    public async Task<IActionResult> GetAllDevicesSyncStatus(string siteCode)
    {
        try
        {
            // Verify site exists
            var site = await _context.Sites.FirstOrDefaultAsync(s => s.SiteCode == siteCode);
            if (site == null)
            {
                return NotFound($"Site '{siteCode}' not found");
            }

            var devices = await _context.Devices
                .Where(d => d.SiteId == site.Id)
                .Select(d => new
                {
                    d.Id,
                    d.SerialNumber,
                    d.DeviceName,
                    d.IsOnline,
                    d.WhitelistStartSync,
                    d.WhitelistSyncStatus,
                    d.WhitelistSyncStartedAt,
                    d.WhitelistSyncBatchesSent,
                    d.WhitelistSyncTotalBatches,
                    Progress = d.WhitelistSyncTotalBatches > 0
                        ? (double)d.WhitelistSyncBatchesSent / d.WhitelistSyncTotalBatches * 100
                        : 0
                })
                .ToListAsync();

            return Ok(new { siteCode, devices });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting sync status for all devices in site {SiteCode}", siteCode);
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

}
