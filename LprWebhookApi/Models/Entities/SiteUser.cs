using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("site_users")]
public class SiteUser
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("user_name")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("role")]
    public string Role { get; set; } = string.Empty; // 'admin', 'manager', 'security', 'viewer'

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [MaxLength(255)]
    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<Whitelist> CreatedWhitelists { get; set; } = new List<Whitelist>();
    public virtual ICollection<Whitelist> ApprovedWhitelists { get; set; } = new List<Whitelist>();
    public virtual ICollection<SiteConfiguration> UpdatedConfigurations { get; set; } = new List<SiteConfiguration>();
}
