using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("device_heartbeats")]
public class DeviceHeartbeat
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

    [MaxLength(20)]
    [Column("heartbeat_type")]
    public string? HeartbeatType { get; set; } // 'normal', 'comet'

    [MaxLength(50)]
    [Column("user_name")]
    public string? UserName { get; set; }

    [MaxLength(50)]
    [Column("password")]
    public string? Password { get; set; }

    [Column("channel_num")]
    public int? ChannelNum { get; set; }

    [Column("received_at")]
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}

[Table("io_trigger_events")]
public class IoTriggerEvent
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

    [Column("source")]
    public int? Source { get; set; } // 0-4 (IO inputs 1-4, TCP)

    [Column("value")]
    public int? Value { get; set; } // trigger state

    [Column("triggered_at")]
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}

[Table("serial_data_logs")]
public class SerialDataLog
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

    [Column("serial_channel")]
    public int? SerialChannel { get; set; }

    [Column("data_base64")]
    public string? DataBase64 { get; set; }

    [Column("data_length")]
    public int? DataLength { get; set; }

    [Column("received_at")]
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}

[Table("screenshots")]
public class Screenshot
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

    [Column("image_base64")]
    public string? ImageBase64 { get; set; }

    [Column("image_length")]
    public int? ImageLength { get; set; }

    [MaxLength(50)]
    [Column("trigger_source")]
    public string? TriggerSource { get; set; } // 'manual', 'comet', 'recognition_response'

    [Column("captured_at")]
    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    [ForeignKey("DeviceId")]
    public virtual Device Device { get; set; } = null!;
}
