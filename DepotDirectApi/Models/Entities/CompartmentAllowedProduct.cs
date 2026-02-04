using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DepotDirectApi.Models.Entities;

[Table("compartment_allowed_products", Schema = "depotdirect")]
public class CompartmentAllowedProduct
{
    [Required]
    [Column("compartment_id")]
    public int CompartmentId { get; set; }

    [Required]
    [Column("product_id")]
    public int ProductId { get; set; }

    // Navigation properties
    [ForeignKey("CompartmentId")]
    public virtual TrailerCompartment Compartment { get; set; } = null!;

    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}