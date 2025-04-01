using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Country
{
    [Key]
    public int Id
    { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name
    { get; set; } = null!;

    public virtual ICollection<Province> Provinces 
    { get; set; } = [];
}
