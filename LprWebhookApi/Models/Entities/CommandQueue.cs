using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace LprWebhookApi.Models.Entities;

[Table("command_queue")]
public class CommandQueue
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

    [MaxLength(50)]
    [Column("command_type")]
    public string? CommandType { get; set; } // 'gate_open', 'screenshot', 'manual_trigger', 'whitelist_add', etc.

    [Column("command_data", TypeName = "jsonb")]
    public JsonDocument? CommandData { get; set; }

    [Column("is_processed")]
    public bool IsProcessed { get; set; } = false;

    [Column("priority")]
    public int Priority { get; set; } = 0;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("processed_at")]
    public DateTime? ProcessedAt { get; set; }

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}

[Table("response_logs")]
public class ResponseLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Column("device_id")]
    public int? DeviceId { get; set; } // NULL for site-level operations

    [MaxLength(50)]
    [Column("request_type")]
    public string? RequestType { get; set; }

    [Column("request_data", TypeName = "jsonb")]
    public JsonDocument? RequestData { get; set; }

    [Column("response_data", TypeName = "jsonb")]
    public JsonDocument? ResponseData { get; set; }

    [Column("processing_time_ms")]
    public int? ProcessingTimeMs { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device? Device { get; set; }
}

[Table("site_configurations")]
public class SiteConfiguration
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("config_key")]
    public string ConfigKey { get; set; } = string.Empty;

    [Column("config_value")]
    public string? ConfigValue { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("updated_by")]
    public int? UpdatedBy { get; set; } // FK: site_users.id

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    public virtual SiteUser? UpdatedByUser { get; set; }
}
