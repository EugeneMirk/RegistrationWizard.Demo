using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class User
{
    [Key]
    public int Id
    { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email
    { get; set; }

    [Required]
    [MaxLength(64)]
    public required string PasswordHash
    { get; set; }

    [ForeignKey(nameof(Country))]
    public int CountryId
    { get; set; }

    [ForeignKey(nameof(Province))]   
    public int ProvinceId
    { get; set; }

    public Country Country
    { get; set; } = null!;

    public Province Province
    { get; set; } = null!;

    public DateTime Created
    { get; set; }
}
