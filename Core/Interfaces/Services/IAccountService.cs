using Core.DTOs.AccountDTOs;
using Core.DTOs.AccountDTOs.Requests;

namespace Core.Interfaces.Services;

public interface IAccountService
{
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken token);

    Task<UserRegistrationResult> RegisterAsync(UserRegistrationRequest user, CancellationToken token);
}
