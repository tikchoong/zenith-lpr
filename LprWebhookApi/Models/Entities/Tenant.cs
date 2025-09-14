using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("tenants")]
public class Tenant
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [MaxLength(20)]
    [Column("tenant_code")]
    public string? TenantCode { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("tenant_name")]
    public string TenantName { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("unit_number")]
    public string? UnitNumber { get; set; }

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [MaxLength(100)]
    [Column("emergency_contact")]
    public string? EmergencyContact { get; set; }

    [MaxLength(20)]
    [Column("emergency_phone")]
    public string? EmergencyPhone { get; set; }

    [Column("move_in_date")]
    public DateOnly? MoveInDate { get; set; }

    [Column("move_out_date")]
    public DateOnly? MoveOutDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<Whitelist> Whitelists { get; set; } = new List<Whitelist>();
    public virtual ICollection<EntryLog> EntryLogs { get; set; } = new List<EntryLog>();
}
