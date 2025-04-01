using Core.DTOs.AccountDTOs;
using Core.DTOs.AccountDTOs.Requests;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AccountService> _logger;
    private readonly IPasswordService _passwordService;

    private const string EmailExistsCachePrefix = "EmailExists_";

    public AccountService(
        IUserRepository userRepository,
        IMemoryCache cache,
        ILogger<AccountService> logger,
        IPasswordService passwordHasher)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordService = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task<UserRegistrationResult> RegisterAsync(
        UserRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsPasswordValid(request.Password))
            {
                _logger.LogWarning("Password validation failed for email {Email}", request.Email);

                return UserRegistrationResult.Fail(
                    "Password must contain at least 1 letter and 1 digit",
                    ErrorType.ValidationError);
            }

            var emailExists = await IsEmailExistsAsync(request.Email, cancellationToken);

            if (emailExists)
            {
                _logger.LogWarning("Duplicate email registration attempt: {Email}", request.Email);

                return UserRegistrationResult.Fail(
                    "Email is already registered",
                    ErrorType.DuplicateEmail);
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = _passwordService.HashPassword(request.Password),
                CountryId = request.CountryId,
                ProvinceId = request.ProvinceId,
                Created = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user, cancellationToken);

            _cache.Set($"{EmailExistsCachePrefix}{request.Email}", true);

            _logger.LogInformation("New user registered with ID {UserId}", user.Id);

            return UserRegistrationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email {Email}", request.Email);

            return UserRegistrationResult.Fail(
                "An error occurred during registration",
                ErrorType.DatabaseError);
        }
    }

    public async Task<bool> IsEmailAvailableAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return !await IsEmailExistsAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability for {Email}", email);
            throw;
        }
    }

    private async Task<bool> IsEmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue($"{EmailExistsCachePrefix}{email}", out bool existsInCache))
        {
            return existsInCache;
        }

        var existsInDb = await _userRepository.IsEmailExistsAsync(email, cancellationToken);

        if (existsInDb)
        {
            _cache.Set($"{EmailExistsCachePrefix}{email}", true);
        }

        return existsInDb;
    }

    private static bool IsPasswordValid(string password)
    {
        return password.Length >= 2 &&
               password.Any(char.IsLetter) &&
               password.Any(char.IsDigit);
    }
}