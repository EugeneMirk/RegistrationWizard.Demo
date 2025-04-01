using Api.Controllers;
using Core.DTOs.AccountDTOs;
using Core.DTOs.AccountDTOs.Requests;
using Core.Interfaces.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Api;

public class AccountsControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock = new();
    private readonly Mock<ILogger<AccountsController>> _loggerMock = new();
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _controller = new AccountsController(_loggerMock.Object, _accountServiceMock.Object);
    }

    [Fact]
    public async Task IsEmailAvailable_ReturnsTrue_WhenEmailIsAvailable()
    {
        const string testEmail = "test@example.com";

        _accountServiceMock
            .Setup(x => x.IsEmailAvailableAsync(testEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.IsEmailAvailable(testEmail);

        var actionResult = result.Should().BeOfType<ActionResult<bool>>().Subject;

        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(true);
    }

    [Fact]
    public async Task IsEmailAvailable_Returns499_WhenOperationCanceled()
    {
        const string testEmail = "test@example.com";
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _accountServiceMock
            .Setup(x => x.IsEmailAvailableAsync(testEmail, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var result = await _controller.IsEmailAvailable(testEmail, cts.Token);

        result.Result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(499);
    }

    [Fact]
    public async Task Register_Returns200_WhenRegistrationSuccessful()
    {
        var request = new UserRegistrationRequest 
        {
            Email = "valid@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword= "P@ssw0rd!"
        };

        _accountServiceMock
            .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRegistrationResult
            { 
                IsSuccess = true 
            });

        var result = await _controller.Register(request);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task Register_Returns409_WhenEmailAlreadyRegistered()
    {
        var request = new UserRegistrationRequest 
        {
            Email = "duplicate@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        _accountServiceMock
            .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRegistrationResult
            {
                IsSuccess = false,
                ErrorType = ErrorType.DuplicateEmail
            });

        var result = await _controller.Register(request);

        result.Should().BeOfType<ConflictObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Status.Should().Be(409);
    }

    [Fact]
    public async Task Register_Returns500_WhenServerErrorOccurs()
    {
        var request = new UserRegistrationRequest 
        {
            Email = "test@example.com", 
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        _accountServiceMock
            .Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database failure"));

        var result = await _controller.Register(request);

        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Register_PropagatesCancellationToken()
    {

        var cts = new CancellationTokenSource();
        var request = new UserRegistrationRequest
        { 
            Email = "test@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        await _controller.Register(request, cts.Token);

        _accountServiceMock.Verify(
            x => x.RegisterAsync(request, cts.Token),
            Times.Once);
    }
}