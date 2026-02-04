using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace DepotDirectApi.Models.Entities;

[Table("hauliers", Schema = "depotdirect")]
public class Haulier
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("region_id")]
    public int RegionId { get; set; }

    [Required]
    [Column("haulier_code")]
    public string HaulierCode { get; set; } = string.Empty;

    [Required]
    [Column("haulier_name")]
    public string HaulierName { get; set; } = string.Empty;

    [Column("tax_id")]
    public string? TaxId { get; set; }

    [Column("contract_number")]
    public string? ContractNumber { get; set; }

    [Column("contract_expiry")]
    public DateTime? ContractExpiry { get; set; }

    [Column("contact_name")]
    public string? ContactName { get; set; }

    [Column("contact_email")]
    public string? ContactEmail { get; set; }

    [Column("contact_phone")]
    public string? ContactPhone { get; set; }

    [Column("active")]
    public bool Active { get; set; } = true;

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    [ForeignKey("RegionId")]
    public virtual Region Region { get; set; } = null!;

    public virtual ICollection<Tractor> Tractors { get; set; } = new List<Tractor>();
    public virtual ICollection<Trailer> Trailers { get; set; } = new List<Trailer>();
}