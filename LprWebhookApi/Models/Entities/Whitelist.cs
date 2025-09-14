using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("whitelists")]
public class Whitelist
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; } // NULL = all devices in site

    [Column("tenant_id")]
    public int? TenantId { get; set; } // NULL for visitors/staff

    [Column("staff_id")]
    public int? StaffId { get; set; } // NULL for tenants/visitors

    [Required]
    [MaxLength(20)]
    [Column("license_plate")]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("entry_type")]
    public string EntryType { get; set; } = "tenant"; // 'tenant', 'visitor', 'staff', 'temporary'

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("is_blacklist")]
    public bool IsBlacklist { get; set; } = false;

    // Visitor-specific fields
    [MaxLength(100)]
    [Column("visitor_name")]
    public string? VisitorName { get; set; }

    [MaxLength(20)]
    [Column("visitor_phone")]
    public string? VisitorPhone { get; set; }

    [MaxLength(100)]
    [Column("visitor_company")]
    public string? VisitorCompany { get; set; }

    [Column("visit_purpose")]
    public string? VisitPurpose { get; set; }

    // Time-based access control
    [Column("enable_time")]
    public DateTime? EnableTime { get; set; }

    [Column("expiry_time")]
    public DateTime? ExpiryTime { get; set; }

    [Column("max_entries")]
    public int? MaxEntries { get; set; }

    [Column("current_entries")]
    public int CurrentEntries { get; set; } = 0;

    // Recurring access (for tenants/staff)
    [Column("is_recurring")]
    public bool IsRecurring { get; set; } = false;

    [MaxLength(50)]
    [Column("recurring_pattern")]
    public string? RecurringPattern { get; set; } // 'daily', 'weekly', 'monthly', 'weekdays'

    [Column("recurring_start_time")]
    public TimeOnly? RecurringStartTime { get; set; }

    [Column("recurring_end_time")]
    public TimeOnly? RecurringEndTime { get; set; }

    // Device-specific access (NULL = all devices in site)
    [Column("allowed_devices")]
    public int[]? AllowedDevices { get; set; } // Array of device IDs

    // Audit fields
    [Column("created_by")]
    public int? CreatedBy { get; set; } // FK: site_users.id

    [Column("approved_by")]
    public int? ApprovedBy { get; set; } // FK: site_users.id

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device? Device { get; set; }

    [ForeignKey("TenantId")]
    public virtual Tenant? Tenant { get; set; }

    [ForeignKey("StaffId")]
    public virtual SiteStaff? Staff { get; set; }

    [ForeignKey("CreatedBy")]
    public virtual SiteUser? CreatedByUser { get; set; }

    [ForeignKey("ApprovedBy")]
    public virtual SiteUser? ApprovedByUser { get; set; }

    public virtual ICollection<EntryLog> EntryLogs { get; set; } = new List<EntryLog>();
}
