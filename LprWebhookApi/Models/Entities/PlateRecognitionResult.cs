using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LprWebhookApi.Models.Entities;

[Table("plate_recognition_results")]
public class PlateRecognitionResult
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [Column("device_id")]
    public int DeviceId { get; set; }

    [Required]
    [Column("plate_id")]
    public int PlateId { get; set; } // from camera (plateid field)

    [MaxLength(20)]
    [Column("license_plate")]
    public string? LicensePlate { get; set; }

    [Column("confidence")]
    public int? Confidence { get; set; }

    [Column("color_type")]
    public int? ColorType { get; set; }

    [Column("plate_type")]
    public int? PlateType { get; set; }

    [Column("direction")]
    public int? Direction { get; set; }

    [Column("trigger_type")]
    public int? TriggerType { get; set; }

    [Column("is_offline")]
    public bool IsOffline { get; set; } = false;

    [Column("is_fake_plate")]
    public bool? IsFakePlate { get; set; }

    [Column("plate_true_width")]
    public int? PlateTrueWidth { get; set; }

    [Column("plate_distance")]
    public int? PlateDistance { get; set; }

    // Location data (JSON)
    [Column("plate_location", TypeName = "jsonb")]
    public JsonDocument? PlateLocation { get; set; }

    [Column("car_location", TypeName = "jsonb")]
    public JsonDocument? CarLocation { get; set; }

    // Vehicle info
    [Column("car_brand")]
    public int? CarBrand { get; set; }

    [Column("car_year")]
    public int? CarYear { get; set; }

    [Column("car_type")]
    public int? CarType { get; set; }

    [MaxLength(20)]
    [Column("feature_code")]
    public string? FeatureCode { get; set; }

    // Timing
    [Column("recognition_timestamp")]
    public DateTime? RecognitionTimestamp { get; set; }

    [Column("time_used")]
    public int? TimeUsed { get; set; }

    [Column("usec")]
    public int? Usec { get; set; }

    // Images
    [MaxLength(500)]
    [Column("image_path")]
    public string? ImagePath { get; set; }

    [Column("image_file_base64")]
    public string? ImageFileBase64 { get; set; }

    [Column("image_file_length")]
    public int? ImageFileLength { get; set; }

    [Column("image_fragment_base64")]
    public string? ImageFragmentBase64 { get; set; }

    [Column("image_fragment_length")]
    public int? ImageFragmentLength { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;

    public virtual ICollection<EntryLog> EntryLogs { get; set; } = new List<EntryLog>();
}
