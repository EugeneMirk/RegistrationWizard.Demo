using Core.Validation;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.AccountDTOs.Requests;

public record UserRegistrationRequest
{
    [Required]
    [EmailAddress]
    public required string Email { set; get; }

    [Required]
    [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*[0-9]).+$",
        ErrorMessage = "Password must contain at least 1 letter and 1 digit")]
    [StringLength(100, MinimumLength = 2)]
    public required string Password
    { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public required string ConfirmPassword
    { get; set; }

    [Required]
    public int CountryId
    { get; set; }

    [Required]
    public int ProvinceId
    { get; set; }

    [Required]
    [MustBeTrue]
    public bool AgreeToTerms
    { get; set; }
}
