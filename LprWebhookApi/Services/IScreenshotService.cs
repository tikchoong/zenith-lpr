using LprWebhookApi.Models.DTOs;

namespace LprWebhookApi.Services;

public interface IScreenshotService
{
    Task<bool> SaveScreenshotAsync(int siteId, string deviceIp, ScreenshotCaptureRequest request);
    Task<ScreenshotDetailResponse?> GetScreenshotAsync(int id);
    Task<ScreenshotResponse?> GetScreenshotMetadataAsync(int id);
    Task<List<ScreenshotResponse>> GetScreenshotsByPlateAsync(string licensePlate);
    Task<List<ScreenshotResponse>> GetScreenshotsByDeviceAsync(int deviceId, DateTime? fromDate = null);
    Task MarkScreenshotTimeoutAsync(int screenshotId);
    Task<byte[]?> GetScreenshotImageAsync(int id);
}
