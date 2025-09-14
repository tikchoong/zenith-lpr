using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("sites")]
public class Site
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("site_code")]
    public string SiteCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    [Column("site_name")]
    public string SiteName { get; set; } = string.Empty;

    [Column("address")]
    public string? Address { get; set; }

    [MaxLength(100)]
    [Column("city")]
    public string? City { get; set; }

    [MaxLength(50)]
    [Column("state")]
    public string? State { get; set; }

    [MaxLength(20)]
    [Column("postal_code")]
    public string? PostalCode { get; set; }

    [MaxLength(50)]
    [Column("country")]
    public string Country { get; set; } = "Malaysia";

    [MaxLength(50)]
    [Column("timezone")]
    public string Timezone { get; set; } = "Asia/Kuala_Lumpur";

    [Column("max_devices")]
    public int MaxDevices { get; set; } = 10;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(100)]
    [Column("site_manager_name")]
    public string? SiteManagerName { get; set; }

    [MaxLength(20)]
    [Column("site_manager_phone")]
    public string? SiteManagerPhone { get; set; }

    [MaxLength(100)]
    [Column("site_manager_email")]
    public string? SiteManagerEmail { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<SiteUser> SiteUsers { get; set; } = new List<SiteUser>();
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    public virtual ICollection<SiteStaff> SiteStaff { get; set; } = new List<SiteStaff>();
    public virtual ICollection<Whitelist> Whitelists { get; set; } = new List<Whitelist>();
    public virtual ICollection<EntryLog> EntryLogs { get; set; } = new List<EntryLog>();
    public virtual ICollection<PlateRecognitionResult> PlateRecognitionResults { get; set; } = new List<PlateRecognitionResult>();
    public virtual ICollection<SiteConfiguration> SiteConfigurations { get; set; } = new List<SiteConfiguration>();
}
