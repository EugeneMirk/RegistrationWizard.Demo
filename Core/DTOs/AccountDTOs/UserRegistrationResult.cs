namespace Core.DTOs.AccountDTOs;

public record UserRegistrationResult
{
    public bool IsSuccess
    { get; set; }

    public ErrorType? ErrorType
    { get; set; } = null;

    public string? ErrorMessage
    { get; set; } = null;

    public UserRegistrationResult()
    { }

    public UserRegistrationResult(bool isSuccess, ErrorType? errorType = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static UserRegistrationResult Success()
        => new(true);

    public static UserRegistrationResult Fail(string message, ErrorType errorType)
        => new(false, errorType, message);
}
