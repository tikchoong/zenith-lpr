using LprWebhookApi.Data;
using LprWebhookApi.Models;
using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace LprWebhookApi.Services;

public class ScreenshotService : IScreenshotService
{
    private readonly LprDbContext _context;
    private readonly ILogger<ScreenshotService> _logger;
    private const int SCREENSHOT_TIMEOUT_SECONDS = 30;

    public ScreenshotService(LprDbContext context, ILogger<ScreenshotService> logger)
    {
        _context = context;
        _logger = logger;
    }



    public async Task<bool> SaveScreenshotAsync(int siteId, string deviceIp, ScreenshotCaptureRequest request)
    {
        try
        {
            // Check if there's actual image data
            if (request.TriggerImage?.ImageFileLen == null || request.TriggerImage.ImageFileLen <= 0 ||
                string.IsNullOrEmpty(request.TriggerImage.ImageFile))
            {
                Log.Debug("No image data provided, skipping screenshot save for device IP {DeviceIp}", deviceIp);
                return false;
            }

            // Find the device
            var deviceIpAddress = System.Net.IPAddress.Parse(deviceIp);
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.SiteId == siteId && d.IpAddress!.Equals(deviceIpAddress));

            if (device == null)
            {
                Log.Warning("Device not found for IP {DeviceIp} in site {SiteId}", deviceIp, siteId);
                return false;
            }

            // Find the most recent plate recognition for this device to link the screenshot
            var recentPlateRecognition = await _context.PlateRecognitionResults
                .Include(p => p.EntryLogs)
                .Where(p => p.DeviceId == device.Id)
                .OrderByDescending(p => p.RecognitionTimestamp)
                .FirstOrDefaultAsync();

            if (recentPlateRecognition == null)
            {
                Log.Warning("No recent plate recognition found for device {DeviceId}", device.Id);
                return false;
            }

            // Get the entry status from the most recent entry log
            var entryStatus = recentPlateRecognition.EntryLogs
                .OrderByDescending(e => e.CreatedAt)
                .FirstOrDefault()?.EntryStatus ?? "unknown";

            // Create new screenshot record with actual image data
            var screenshot = new PlateRecognitionScreenshot
            {
                PlateRecognitionId = recentPlateRecognition.Id,
                SiteId = siteId,
                DeviceId = device.Id,
                LicensePlate = recentPlateRecognition.LicensePlate ?? "",
                RecognitionResult = entryStatus,
                ScreenshotStatus = "completed",
                RequestedAt = DateTime.UtcNow,
                ReceivedAt = DateTime.UtcNow,
                TriggerSource = "plate_recognition",
                ImageBase64 = request.TriggerImage.ImageFile,
                ImageLength = request.TriggerImage.ImageFileLen,
                CameraIp = deviceIp,
                ImageFormat = "jpeg"
            };

            _context.PlateRecognitionScreenshots.Add(screenshot);

            // Update device status
            device.ScreenshotCaptureStatus = "completed";
            device.LastScreenshotRequest = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            Log.Information("Screenshot saved for plate recognition {PlateRecognitionId}, screenshot ID {ScreenshotId}, size {ImageLength} bytes",
                recentPlateRecognition.Id, screenshot.Id, screenshot.ImageLength);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving screenshot for site {SiteId}, device IP {DeviceIp}", siteId, deviceIp);
            return false;
        }
    }

    public async Task<ScreenshotDetailResponse?> GetScreenshotAsync(int id)
    {
        var screenshot = await _context.PlateRecognitionScreenshots
            .Include(s => s.Device)
            .Include(s => s.Site)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screenshot == null) return null;

        return new ScreenshotDetailResponse
        {
            Id = screenshot.Id,
            PlateRecognitionId = screenshot.PlateRecognitionId,
            LicensePlate = screenshot.LicensePlate,
            RecognitionResult = screenshot.RecognitionResult,
            ScreenshotStatus = screenshot.ScreenshotStatus,
            RequestedAt = screenshot.RequestedAt,
            ReceivedAt = screenshot.ReceivedAt,
            DeviceName = screenshot.Device.DeviceName ?? "",
            SiteCode = screenshot.Site.SiteCode,
            ImageLength = screenshot.ImageLength,
            ImageFormat = screenshot.ImageFormat,
            ImageBase64 = screenshot.ImageBase64,
            ImageUrl = $"/api/screenshots/{id}/image",
            DownloadUrl = $"/api/screenshots/{id}/download",
            ResponseTimeSeconds = screenshot.ReceivedAt.HasValue
                ? (screenshot.ReceivedAt.Value - screenshot.RequestedAt).TotalSeconds
                : null
        };
    }

    public async Task<ScreenshotResponse?> GetScreenshotMetadataAsync(int id)
    {
        var screenshot = await _context.PlateRecognitionScreenshots
            .Include(s => s.Device)
            .Include(s => s.Site)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screenshot == null) return null;

        return new ScreenshotResponse
        {
            Id = screenshot.Id,
            PlateRecognitionId = screenshot.PlateRecognitionId,
            LicensePlate = screenshot.LicensePlate,
            RecognitionResult = screenshot.RecognitionResult,
            ScreenshotStatus = screenshot.ScreenshotStatus,
            RequestedAt = screenshot.RequestedAt,
            ReceivedAt = screenshot.ReceivedAt,
            DeviceName = screenshot.Device.DeviceName ?? "",
            SiteCode = screenshot.Site.SiteCode,
            ImageLength = screenshot.ImageLength,
            ImageFormat = screenshot.ImageFormat,
            ImageUrl = $"/api/screenshots/{id}/image",
            DownloadUrl = $"/api/screenshots/{id}/download",
            ResponseTimeSeconds = screenshot.ReceivedAt.HasValue
                ? (screenshot.ReceivedAt.Value - screenshot.RequestedAt).TotalSeconds
                : null
        };
    }

    public async Task<List<ScreenshotResponse>> GetScreenshotsByPlateAsync(string licensePlate)
    {
        var screenshots = await _context.PlateRecognitionScreenshots
            .Include(s => s.Device)
            .Include(s => s.Site)
            .Where(s => s.LicensePlate == licensePlate)
            .OrderByDescending(s => s.RequestedAt)
            .ToListAsync();

        return screenshots.Select(s => new ScreenshotResponse
        {
            Id = s.Id,
            PlateRecognitionId = s.PlateRecognitionId,
            LicensePlate = s.LicensePlate,
            RecognitionResult = s.RecognitionResult,
            ScreenshotStatus = s.ScreenshotStatus,
            RequestedAt = s.RequestedAt,
            ReceivedAt = s.ReceivedAt,
            DeviceName = s.Device.DeviceName ?? "",
            SiteCode = s.Site.SiteCode,
            ImageLength = s.ImageLength,
            ImageFormat = s.ImageFormat,
            ImageUrl = $"/api/screenshots/{s.Id}/image",
            DownloadUrl = $"/api/screenshots/{s.Id}/download",
            ResponseTimeSeconds = s.ReceivedAt.HasValue
                ? (s.ReceivedAt.Value - s.RequestedAt).TotalSeconds
                : null
        }).ToList();
    }

    public async Task<List<ScreenshotResponse>> GetScreenshotsByDeviceAsync(int deviceId, DateTime? fromDate = null)
    {
        var query = _context.PlateRecognitionScreenshots
            .Include(s => s.Device)
            .Include(s => s.Site)
            .Where(s => s.DeviceId == deviceId);

        if (fromDate.HasValue)
        {
            query = query.Where(s => s.RequestedAt >= fromDate.Value);
        }

        var screenshots = await query
            .OrderByDescending(s => s.RequestedAt)
            .ToListAsync();

        return screenshots.Select(s => new ScreenshotResponse
        {
            Id = s.Id,
            PlateRecognitionId = s.PlateRecognitionId,
            LicensePlate = s.LicensePlate,
            RecognitionResult = s.RecognitionResult,
            ScreenshotStatus = s.ScreenshotStatus,
            RequestedAt = s.RequestedAt,
            ReceivedAt = s.ReceivedAt,
            DeviceName = s.Device.DeviceName ?? "",
            SiteCode = s.Site.SiteCode,
            ImageLength = s.ImageLength,
            ImageFormat = s.ImageFormat,
            ImageUrl = $"/api/screenshots/{s.Id}/image",
            DownloadUrl = $"/api/screenshots/{s.Id}/download",
            ResponseTimeSeconds = s.ReceivedAt.HasValue
                ? (s.ReceivedAt.Value - s.RequestedAt).TotalSeconds
                : null
        }).ToList();
    }

    public async Task MarkScreenshotTimeoutAsync(int screenshotId)
    {
        var screenshot = await _context.PlateRecognitionScreenshots.FindAsync(screenshotId);
        if (screenshot != null && screenshot.ScreenshotStatus == "pending")
        {
            screenshot.ScreenshotStatus = "timeout";
            screenshot.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            Log.Warning("Screenshot {ScreenshotId} marked as timeout", screenshotId);
        }
    }

    public async Task<byte[]?> GetScreenshotImageAsync(int id)
    {
        var screenshot = await _context.PlateRecognitionScreenshots
            .FirstOrDefaultAsync(s => s.Id == id);

        if (screenshot == null || string.IsNullOrEmpty(screenshot.ImageBase64))
        {
            return null;
        }

        try
        {
            return Convert.FromBase64String(screenshot.ImageBase64);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error converting Base64 image for screenshot {ScreenshotId}", id);
            return null;
        }
    }
}
