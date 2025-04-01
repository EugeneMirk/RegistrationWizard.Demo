using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class Province
{
    [Key]
    public int Id
    { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Name
    { get; set; }

    [ForeignKey(nameof(Country))]
    public int CountryId
    { get; set; }

    public virtual Country Country
    { get; set; } = null!;
}
