using LprWebhookApi.Data;
using LprWebhookApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;

namespace LprWebhookApi.Services;

public class WhitelistSyncService
{
    private readonly LprDbContext _context;
    private const int BATCH_SIZE = 5;
    private const int SYNC_TIMEOUT_MINUTES = 30;

    public WhitelistSyncService(LprDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Checks if device needs whitelist sync and processes it
    /// </summary>
    public async Task<bool> ProcessWhitelistSync(int deviceId)
    {
        Log.Information("ProcessWhitelistSync called for device {DeviceId}", deviceId);

        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            Log.Warning("Device {DeviceId} not found", deviceId);
            return false;
        }

        Log.Information("Device {DeviceId} found. WhitelistStartSync: {WhitelistStartSync}, Status: {Status}",
            deviceId, device.WhitelistStartSync, device.WhitelistSyncStatus);

        if (!device.WhitelistStartSync)
        {
            Log.Information("Whitelist sync disabled for device {DeviceId}", deviceId);
            return false;
        }

        // Check for timeout
        if (await IsWhitelistSyncTimedOut(device))
        {
            Log.Warning("Whitelist sync timed out for device {DeviceId}", deviceId);
            await MarkSyncAsFailed(device, "Sync timed out");
            return false;
        }

        // Check if sync is already in progress
        if (IsWhitelistSyncInProgress(device))
        {
            Log.Information("Whitelist sync already in progress for device {DeviceId}", deviceId);
            return false;
        }

        // Start new sync
        Log.Information("Starting whitelist sync for device {DeviceId}", deviceId);
        await StartWhitelistSync(device);
        return true;
    }

    /// <summary>
    /// Starts a new whitelist sync process
    /// </summary>
    private async Task StartWhitelistSync(Device device)
    {
        Log.Information("Starting whitelist sync for device {DeviceId}", device.Id);

        // Get all active whitelist entries for this device's site
        var whitelistEntries = await _context.Whitelists
            .Where(w => w.SiteId == device.SiteId && w.IsEnabled && !w.IsBlacklist)
            .Select(w => new
            {
                w.LicensePlate,
                w.EnableTime,
                w.ExpiryTime,
                w.IsBlacklist
            })
            .ToListAsync();

        // Calculate total batches needed
        var totalBatches = (int)Math.Ceiling((double)whitelistEntries.Count / BATCH_SIZE);

        // Update device sync status
        device.WhitelistSyncStartedAt = DateTime.UtcNow;
        device.WhitelistSyncStatus = "clearing";
        device.WhitelistSyncBatchesSent = 0;
        device.WhitelistSyncTotalBatches = totalBatches;

        await _context.SaveChangesAsync();

        // Queue clear command
        await QueueClearWhitelistCommand(device.Id);

        Log.Information("Whitelist sync started for device {DeviceId}. Total batches: {TotalBatches}",
            device.Id, totalBatches);
    }

    /// <summary>
    /// Processes the next step in whitelist sync
    /// </summary>
    public async Task ProcessNextSyncStep(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null || !device.WhitelistStartSync)
        {
            return;
        }

        switch (device.WhitelistSyncStatus)
        {
            case "clearing":
                await ProcessClearingStep(device);
                break;
            case "adding":
                await ProcessAddingStep(device);
                break;
        }
    }

    /// <summary>
    /// Processes the clearing step - moves to adding phase
    /// </summary>
    private async Task ProcessClearingStep(Device device)
    {
        Log.Information("Moving to adding phase for device {DeviceId}", device.Id);

        device.WhitelistSyncStatus = "adding";
        device.WhitelistSyncBatchesSent = 0;
        await _context.SaveChangesAsync();

        // Start sending first batch
        await SendNextWhitelistBatch(device);
    }

    /// <summary>
    /// Processes the adding step - sends next batch or completes sync
    /// </summary>
    private async Task ProcessAddingStep(Device device)
    {
        if (device.WhitelistSyncBatchesSent >= device.WhitelistSyncTotalBatches)
        {
            // Sync complete
            await CompleteSyncProcess(device);
        }
        else
        {
            // Send next batch
            await SendNextWhitelistBatch(device);
        }
    }

    /// <summary>
    /// Sends the next batch of whitelist entries
    /// </summary>
    private async Task SendNextWhitelistBatch(Device device)
    {
        var skip = device.WhitelistSyncBatchesSent * BATCH_SIZE;

        var whitelistBatch = await _context.Whitelists
            .Where(w => w.SiteId == device.SiteId && w.IsEnabled && !w.IsBlacklist)
            .Skip(skip)
            .Take(BATCH_SIZE)
            .Select(w => new
            {
                w.LicensePlate,
                w.EnableTime,
                w.ExpiryTime,
                w.IsBlacklist
            })
            .ToListAsync();

        if (whitelistBatch.Any())
        {
            await QueueAddWhitelistBatchCommand(device.Id, whitelistBatch);

            device.WhitelistSyncBatchesSent++;
            await _context.SaveChangesAsync();

            Log.Information("Sent whitelist batch {BatchNumber}/{TotalBatches} for device {DeviceId}",
                device.WhitelistSyncBatchesSent, device.WhitelistSyncTotalBatches, device.Id);
        }
        else
        {
            // No more entries, complete sync
            await CompleteSyncProcess(device);
        }
    }

    /// <summary>
    /// Completes the sync process
    /// </summary>
    private async Task CompleteSyncProcess(Device device)
    {
        Log.Information("Completing whitelist sync for device {DeviceId}", device.Id);

        device.WhitelistStartSync = false;
        device.WhitelistSyncStatus = "completed";
        device.WhitelistSyncStartedAt = null;
        device.WhitelistSyncBatchesSent = 0;
        device.WhitelistSyncTotalBatches = 0;

        await _context.SaveChangesAsync();

        Log.Information("Whitelist sync completed for device {DeviceId}", device.Id);
    }

    /// <summary>
    /// Marks sync as failed
    /// </summary>
    private async Task MarkSyncAsFailed(Device device, string reason)
    {
        Log.Warning("Whitelist sync failed for device {DeviceId}: {Reason}", device.Id, reason);

        device.WhitelistStartSync = false;
        device.WhitelistSyncStatus = "failed";

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Checks if sync has timed out
    /// </summary>
    private async Task<bool> IsWhitelistSyncTimedOut(Device device)
    {
        if (device.WhitelistSyncStartedAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - device.WhitelistSyncStartedAt.Value;
            return elapsed.TotalMinutes > SYNC_TIMEOUT_MINUTES;
        }
        return false;
    }

    /// <summary>
    /// Checks if sync is currently in progress
    /// </summary>
    private bool IsWhitelistSyncInProgress(Device device)
    {
        return device.WhitelistSyncStatus == "clearing" || device.WhitelistSyncStatus == "adding";
    }

    /// <summary>
    /// Queues a clear whitelist command
    /// </summary>
    private async Task QueueClearWhitelistCommand(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            Log.Error("Cannot queue clear whitelist command: device {DeviceId} not found", deviceId);
            return;
        }

        var command = new CommandQueue
        {
            SiteId = device.SiteId,
            DeviceId = deviceId,
            CommandType = "whitelist_clear",
            CommandData = JsonDocument.Parse(JsonSerializer.Serialize(new { plate = "" })),
            Priority = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.CommandQueue.Add(command);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Queues an add whitelist batch command
    /// </summary>
    private async Task QueueAddWhitelistBatchCommand(int deviceId, IEnumerable<dynamic> whitelistBatch)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            Log.Error("Cannot queue add whitelist batch: device {DeviceId} not found", deviceId);
            return;
        }

        var whitelistData = whitelistBatch.Select(w => new
        {
            plate = w.LicensePlate,
            enable = 1,
            need_alarm = w.IsBlacklist ? 1 : 0,
            enable_time = w.EnableTime?.ToString("yyyy-MM-dd HH:mm:ss"),
            overdue_time = w.ExpiryTime?.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();

        var command = new CommandQueue
        {
            SiteId = device.SiteId,
            DeviceId = deviceId,
            CommandType = "whitelist_add_batch",
            CommandData = JsonDocument.Parse(JsonSerializer.Serialize(new { whitelist_data = whitelistData })),
            Priority = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.CommandQueue.Add(command);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets sync status for a device
    /// </summary>
    public async Task<object> GetSyncStatus(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return new { error = "Device not found" };
        }

        return new
        {
            deviceId = device.Id,
            syncInProgress = device.WhitelistStartSync,
            status = device.WhitelistSyncStatus ?? "idle",
            startedAt = device.WhitelistSyncStartedAt,
            batchesSent = device.WhitelistSyncBatchesSent,
            totalBatches = device.WhitelistSyncTotalBatches,
            progress = device.WhitelistSyncTotalBatches > 0
                ? (double)device.WhitelistSyncBatchesSent / device.WhitelistSyncTotalBatches * 100
                : 0
        };
    }

    /// <summary>
    /// Triggers a whitelist sync for a device
    /// </summary>
    public async Task<bool> TriggerSync(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return false;
        }

        // Reset any previous sync state
        device.WhitelistStartSync = true;
        device.WhitelistSyncStatus = "idle";
        device.WhitelistSyncStartedAt = null;
        device.WhitelistSyncBatchesSent = 0;
        device.WhitelistSyncTotalBatches = 0;

        await _context.SaveChangesAsync();

        Log.Information("Whitelist sync triggered for device {DeviceId}", deviceId);
        return true;
    }

    /// <summary>
    /// Cancels an ongoing sync
    /// </summary>
    public async Task<bool> CancelSync(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null || !device.WhitelistStartSync)
        {
            return false;
        }

        await MarkSyncAsFailed(device, "Cancelled by user");
        Log.Information("Whitelist sync cancelled for device {DeviceId}", deviceId);
        return true;
    }
}
