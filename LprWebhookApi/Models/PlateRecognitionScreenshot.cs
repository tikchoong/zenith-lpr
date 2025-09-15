using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LprWebhookApi.Models.Entities;

namespace LprWebhookApi.Models;

[Table("plate_recognition_screenshots")]
public class PlateRecognitionScreenshot
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("plate_recognition_id")]
    public int PlateRecognitionId { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [Column("device_id")]
    public int DeviceId { get; set; }

    [Required]
    [Column("image_base64")]
    public string ImageBase64 { get; set; } = string.Empty;

    [Required]
    [Column("image_length")]
    public int ImageLength { get; set; }

    [MaxLength(10)]
    [Column("image_format")]
    public string ImageFormat { get; set; } = "jpeg";

    [Required]
    [MaxLength(20)]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("recognition_result")]
    public string RecognitionResult { get; set; } = string.Empty;

    [Column("confidence_score")]
    public decimal? ConfidenceScore { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("screenshot_status")]
    public string ScreenshotStatus { get; set; } = "pending";

    [Column("requested_at")]
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    [Column("received_at")]
    public DateTime? ReceivedAt { get; set; }

    [MaxLength(45)]
    [Column("camera_ip")]
    public string? CameraIp { get; set; }

    [Required]
    [MaxLength(30)]
    [Column("trigger_source")]
    public string TriggerSource { get; set; } = "plate_recognition";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("PlateRecognitionId")]
    public virtual PlateRecognitionResult PlateRecognition { get; set; } = null!;

    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}
