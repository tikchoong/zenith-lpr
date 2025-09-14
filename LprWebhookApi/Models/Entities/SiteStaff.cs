using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("site_staff")]
public class SiteStaff
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("site_id")]
    public int SiteId { get; set; }

    [MaxLength(20)]
    [Column("staff_code")]
    public string? StaffCode { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("staff_name")]
    public string StaffName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("department")]
    public string? Department { get; set; } // 'Security', 'Maintenance', 'Management'

    [MaxLength(50)]
    [Column("position")]
    public string? Position { get; set; }

    [MaxLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [MaxLength(100)]
    [Column("email")]
    public string? Email { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("start_date")]
    public DateOnly? StartDate { get; set; }

    [Column("end_date")]
    public DateOnly? EndDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SiteId")]
    public virtual Site Site { get; set; } = null!;

    public virtual ICollection<Whitelist> Whitelists { get; set; } = new List<Whitelist>();
    public virtual ICollection<EntryLog> EntryLogs { get; set; } = new List<EntryLog>();
}
