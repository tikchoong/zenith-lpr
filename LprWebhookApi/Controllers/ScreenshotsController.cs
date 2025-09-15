using LprWebhookApi.Models.DTOs;
using LprWebhookApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace LprWebhookApi.Controllers;

[Route("api/screenshots")]
[ApiController]
public class ScreenshotsController : ControllerBase
{
    private readonly IScreenshotService _screenshotService;
    private readonly ILogger<ScreenshotsController> _logger;

    public ScreenshotsController(IScreenshotService screenshotService, ILogger<ScreenshotsController> logger)
    {
        _screenshotService = screenshotService;
        _logger = logger;
    }

    /// <summary>
    /// Get screenshot metadata (without Base64 image data)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ScreenshotResponse>> GetScreenshot(int id)
    {
        var screenshot = await _screenshotService.GetScreenshotMetadataAsync(id);
        if (screenshot == null)
        {
            return NotFound($"Screenshot with ID {id} not found");
        }
        return Ok(screenshot);
    }

    /// <summary>
    /// Get complete screenshot data including Base64 image
    /// </summary>
    [HttpGet("{id}/data")]
    public async Task<ActionResult<ScreenshotDetailResponse>> GetScreenshotData(int id)
    {
        var screenshot = await _screenshotService.GetScreenshotAsync(id);
        if (screenshot == null)
        {
            return NotFound($"Screenshot with ID {id} not found");
        }
        return Ok(screenshot);
    }

    /// <summary>
    /// Get screenshot image directly (for display in browser/img tags)
    /// </summary>
    [HttpGet("{id}/image")]
    [Produces("image/jpeg", "image/png")]
    public async Task<IActionResult> GetScreenshotImage(int id)
    {
        var screenshot = await _screenshotService.GetScreenshotMetadataAsync(id);
        if (screenshot == null)
        {
            return NotFound();
        }

        var imageBytes = await _screenshotService.GetScreenshotImageAsync(id);
        if (imageBytes == null)
        {
            return NotFound("Image data not found");
        }

        var contentType = GetContentType(screenshot.ImageFormat);

        // Add metadata headers
        Response.Headers["X-License-Plate"] = screenshot.LicensePlate;
        Response.Headers["X-Recognition-Result"] = screenshot.RecognitionResult;
        Response.Headers["X-Captured-At"] = screenshot.ReceivedAt?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "";
        Response.Headers["X-Device-Name"] = screenshot.DeviceName;
        Response.Headers["X-Site-Code"] = screenshot.SiteCode;

        // Set cache headers
        Response.Headers["Cache-Control"] = "public, max-age=3600";
        Response.Headers["ETag"] = $"\"{id}-{screenshot.RequestedAt.Ticks}\"";

        return File(imageBytes, contentType);
    }

    /// <summary>
    /// Download screenshot image (forces download in browser/Swagger)
    /// </summary>
    [HttpGet("{id}/download")]
    [Produces("image/jpeg", "image/png")]
    public async Task<IActionResult> DownloadScreenshot(int id)
    {
        var screenshot = await _screenshotService.GetScreenshotMetadataAsync(id);
        if (screenshot == null)
        {
            return NotFound();
        }

        var imageBytes = await _screenshotService.GetScreenshotImageAsync(id);
        if (imageBytes == null)
        {
            return NotFound("Image data not found");
        }

        var contentType = GetContentType(screenshot.ImageFormat);
        var fileName = $"screenshot-{screenshot.LicensePlate}-{screenshot.RequestedAt:yyyyMMdd-HHmmss}.{screenshot.ImageFormat}";

        return File(imageBytes, contentType, fileName);
    }

    /// <summary>
    /// Get screenshots by license plate
    /// </summary>
    [HttpGet("plate/{licensePlate}")]
    public async Task<ActionResult<List<ScreenshotResponse>>> GetScreenshotsByPlate(string licensePlate)
    {
        var screenshots = await _screenshotService.GetScreenshotsByPlateAsync(licensePlate);
        return Ok(screenshots);
    }

    /// <summary>
    /// Get screenshots by device
    /// </summary>
    [HttpGet("device/{deviceId}")]
    public async Task<ActionResult<List<ScreenshotResponse>>> GetScreenshotsByDevice(int deviceId,
        [FromQuery] DateTime? fromDate = null)
    {
        var screenshots = await _screenshotService.GetScreenshotsByDeviceAsync(deviceId, fromDate);
        return Ok(screenshots);
    }

    private static string GetContentType(string imageFormat)
    {
        return imageFormat.ToLower() switch
        {
            "jpeg" or "jpg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "webp" => "image/webp",
            _ => "image/jpeg"
        };
    }
}
