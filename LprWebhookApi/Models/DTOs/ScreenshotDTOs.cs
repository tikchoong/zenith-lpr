using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LprWebhookApi.Models.DTOs;

// Request DTOs
public class ScreenshotCaptureRequest
{
    [JsonPropertyName("ipaddr")]
    public string IpAddress { get; set; } = string.Empty;

    [JsonPropertyName("TriggerImage")]
    public ScreenshotImageData TriggerImage { get; set; } = new();

    [JsonPropertyName("plateRecognitionId")]
    public int? PlateRecognitionId { get; set; }
}

public class ScreenshotImageData
{
    [JsonPropertyName("imageFile")]
    public string ImageFile { get; set; } = string.Empty;

    [JsonPropertyName("imageFileLen")]
    public int ImageFileLen { get; set; }
}

public class SetFeatureRequest
{
    [Required]
    public bool Enabled { get; set; }
}

// Response DTOs
public class ScreenshotResponse
{
    public int Id { get; set; }
    public int PlateRecognitionId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string RecognitionResult { get; set; } = string.Empty;
    public string ScreenshotStatus { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string SiteCode { get; set; } = string.Empty;
    public int ImageLength { get; set; }
    public string ImageFormat { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public double? ResponseTimeSeconds { get; set; }
}

public class ScreenshotDetailResponse : ScreenshotResponse
{
    public string ImageBase64 { get; set; } = string.Empty;
}

public class DeviceStatusResponse
{
    public int DeviceId { get; set; }
    public string SerialNumber { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string SiteCode { get; set; } = string.Empty;
    public bool IsOnline { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public WhitelistSyncStatus WhitelistSync { get; set; } = new();
    public ScreenshotCaptureStatus ScreenshotCapture { get; set; } = new();
}

public class WhitelistSyncStatus
{
    public bool Enabled { get; set; }
    public string? Status { get; set; }
    public DateTime? LastStarted { get; set; }
    public int BatchesSent { get; set; }
    public int TotalBatches { get; set; }
}

public class ScreenshotCaptureStatus
{
    public bool Enabled { get; set; }
    public string? Status { get; set; }
    public DateTime? LastRequest { get; set; }
}
