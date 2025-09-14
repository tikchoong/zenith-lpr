using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LprWebhookApi.Models.Entities;

[Table("lookup_tables")]
public class LookupTable
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = string.Empty; // e.g., "color_type", "direction", "entry_type", "recurring_pattern"

    [Required]
    [MaxLength(50)]
    [Column("code")]
    public string Code { get; set; } = string.Empty; // The string/code value like "tenant", "daily", "red"

    [Column("numeric_value")]
    public int? NumericValue { get; set; } // For numeric constants like color codes

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty; // Display name like "Tenant", "Daily", "Red"

    [Required]
    [MaxLength(500)]
    [Column("description")]
    public string Description { get; set; } = string.Empty; // Full description

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("sort_order")]
    public int? SortOrder { get; set; }

    [MaxLength(100)]
    [Column("parent_category")]
    public string? ParentCategory { get; set; } // For hierarchical lookups

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
