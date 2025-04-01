using System.ComponentModel.DataAnnotations;

namespace Core.Validation;

public class MustBeTrueAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
        => value is bool b && b;
}
