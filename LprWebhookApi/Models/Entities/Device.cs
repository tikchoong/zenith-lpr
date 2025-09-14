using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace LprWebhookApi.Models.Entities;

[Table("devices")]
public class Device
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("serial_number")]
    public string SerialNumber { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("device_name")]
    public string? DeviceName { get; set; }

    [Column("ip_address")]
    public IPAddress? IpAddress { get; set; }

    [Column("port")]
    public int? Port { get; set; }

    [MaxLength(200)]
    [Column("location_description")]
    public string? LocationDescription { get; set; }

    [Column("is_online")]
    public bool IsOnline { get; set; } = false;

    [Column("last_heartbeat")]
    public DateTime? LastHeartbeat { get; set; }

    [MaxLength(50)]
    [Column("firmware_version")]
    public string? FirmwareVersion { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<PlateRecognitionResult> PlateRecognitionResults { get; set; } = new List<PlateRecognitionResult>();
    public virtual ICollection<DeviceHeartbeat> DeviceHeartbeats { get; set; } = new List<DeviceHeartbeat>();
    public virtual ICollection<IoTriggerEvent> IoTriggerEvents { get; set; } = new List<IoTriggerEvent>();
    public virtual ICollection<SerialDataLog> SerialDataLogs { get; set; } = new List<SerialDataLog>();
    public virtual ICollection<Screenshot> Screenshots { get; set; } = new List<Screenshot>();
    public virtual ICollection<CommandQueue> CommandQueue { get; set; } = new List<CommandQueue>();
    public virtual ICollection<EntryLog> EntryLogs { get; set; } = new List<EntryLog>();
    public virtual ICollection<Whitelist> Whitelists { get; set; } = new List<Whitelist>();
}
