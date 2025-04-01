using Application.Services;
using Core.DTOs.AccountDTOs;
using Core.DTOs.AccountDTOs.Requests;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application;

public class AccountServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IMemoryCache> _cacheMock = new();
    private readonly Mock<ILogger<AccountService>> _loggerMock = new();
    private readonly Mock<IPasswordService> _passwordServiceMock = new();
    private readonly AccountService _accountService;

    public AccountServiceTests()
    {
        _accountService = new AccountService(
            _userRepositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object,
            _passwordServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsSuccess_WhenAllConditionsMet()
    {
        var request = new UserRegistrationRequest
        {
            Email = "valid@test.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
            CountryId = 1,
            ProvinceId = 2
        };

        SetupMocksForSuccessRegistration(request);

        var result = await _accountService.RegisterAsync(request);

        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<User>(u =>
                    u.Email == request.Email &&
                    u.CountryId == request.CountryId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("short", false)]
    [InlineData("12345678", false)]
    [InlineData("lettersOnly", false)]
    [InlineData("Valid123", true)]
    public async Task RegisterAsync_ValidatesPasswordCorrectly(string password, bool expectedValid)
    {
        var request = new UserRegistrationRequest
        {
            Email = "test@test.com",
            Password = password,
            ConfirmPassword = password
        };

        SetupCacheMock(request.Email, false);

        var result = await _accountService.RegisterAsync(request);

        result.IsSuccess.Should().Be(expectedValid);

        if (!expectedValid)
        {
            result.ErrorType.Should().Be(ErrorType.ValidationError);
        }
        else
        {
            _userRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task RegisterAsync_ReturnsDuplicateEmailError_WhenEmailExists()
    {

        var request = new UserRegistrationRequest
        {
            Email = "exists@test.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123"
        };

        SetupCacheMock(request.Email, true);

        var result = await _accountService.RegisterAsync(request);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.DuplicateEmail);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsDatabaseError_WhenRepositoryThrows()
    {
        var request = new UserRegistrationRequest
        {
            Email = "test@test.com",
            Password = "ValidPass123",
            ConfirmPassword = "ValidPass123",
        };

        SetupMocksForSuccessRegistration(request);

        _userRepositoryMock.Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _accountService.RegisterAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorType.Should().Be(ErrorType.DatabaseError);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task IsEmailAvailableAsync_ReturnsCorrectStatus(bool emailExists, bool expectedAvailable)
    {
        const string email = "test@test.com";
        SetupCacheMock(email, emailExists);

        var result = await _accountService.IsEmailAvailableAsync(email);

        result.Should().Be(expectedAvailable);
    }

    [Fact]
    public async Task IsEmailAvailableAsync_UsesCache()
    {
        const string email = "cached@test.com";
        var cacheKey = $"EmailExists_{email}";
        object cachedValue = true;

        _cacheMock.Setup(x => x.TryGetValue(cacheKey, out cachedValue))
            .Returns(true);

        var result = await _accountService.IsEmailAvailableAsync(email);

        result.Should().BeFalse();
        _userRepositoryMock.Verify(x => x.IsEmailExistsAsync(email, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IsEmailAvailableAsync_Throws_WhenRepositoryFails()
    {
        const string email = "error@test.com";

        _userRepositoryMock.Setup(x => x.IsEmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        await Assert.ThrowsAsync<Exception>(() => _accountService.IsEmailAvailableAsync(email));
    }

    private void SetupMocksForSuccessRegistration(UserRegistrationRequest request)
    {
        SetupCacheMock(request.Email, false);

        _passwordServiceMock.Setup(x => x.HashPassword(request.Password))
            .Returns("hashed_password");
    }

    private void SetupCacheMock(string email, bool existsInDb)
    {
        var cacheKey = $"EmailExists_{email}";
        object? outValue = null;

        _cacheMock.Setup(x => x.CreateEntry(cacheKey))
            .Returns(Mock.Of<ICacheEntry>);

        _cacheMock.Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(false);

        _userRepositoryMock.Setup(x => x.IsEmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existsInDb);
    }
}
