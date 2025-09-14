using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("entry_logs")]
public class EntryLog
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

    [Column("tenant_id")]
    public int? TenantId { get; set; } // NULL for non-tenant entries

    [Column("staff_id")]
    public int? StaffId { get; set; } // NULL for non-staff entries

    [Column("whitelist_id")]
    public int? WhitelistId { get; set; } // NULL for unknown plates

    [Column("plate_recognition_id")]
    public int? PlateRecognitionId { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("entry_type")]
    public string? EntryType { get; set; } // 'tenant', 'visitor', 'staff', 'unknown'

    [MaxLength(20)]
    [Column("entry_status")]
    public string? EntryStatus { get; set; } // 'allowed', 'denied', 'expired', 'exceeded_limit'

    // Recognition details
    [Column("confidence")]
    public int? Confidence { get; set; }

    // Entry details
    [Column("entry_time")]
    public DateTime EntryTime { get; set; } = DateTime.UtcNow;

    [Column("exit_time")]
    public DateTime? ExitTime { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    // Additional info
    [Column("gate_opened")]
    public bool GateOpened { get; set; } = false;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;

    [ForeignKey("TenantId")]
    public virtual Tenant? Tenant { get; set; }

    [ForeignKey("StaffId")]
    public virtual SiteStaff? Staff { get; set; }

    [ForeignKey("WhitelistId")]
    public virtual Whitelist? Whitelist { get; set; }

    [ForeignKey("PlateRecognitionId")]
    public virtual PlateRecognitionResult? PlateRecognitionResult { get; set; }
}
