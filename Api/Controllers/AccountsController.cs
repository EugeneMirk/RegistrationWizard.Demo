using Core.DTOs.AccountDTOs;
using Core.DTOs.AccountDTOs.Requests;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

/// <summary>
/// Handles account-related operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountService _accountService;

    public AccountsController(
        ILogger<AccountsController> logger,
        IAccountService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    /// <summary>
    /// Checks if login/email is available for registration
    /// </summary>
    /// <param name="login">Email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns availability status</response>
    /// <response code="400">If login is invalid</response>
    [HttpGet("check-email")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> IsEmailAvailable(
        [FromQuery, Required, EmailAddress] string login,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Checking email availability for {Login}", login);

            var isAvailable = await _accountService.IsEmailAvailableAsync(login, cancellationToken);

            return Ok(isAvailable);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email availability check for {Login} was canceled", login);

            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability for {Login}", login);

            return Problem(
                title: "Server Error",
                detail: "An error occurred while checking email availability",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">Registration data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Registration successful</response>
    /// <response code="400">Invalid registration data</response>
    /// <response code="409">Email already registered</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registration attempt for {Email}", request.Email);

            var result = await _accountService.RegisterAsync(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ErrorType.DuplicateEmail => Conflict(new ProblemDetails
                    {
                        Title = "Email Conflict",
                        Detail = result.ErrorMessage ?? "Email already registered",
                        Status = StatusCodes.Status409Conflict
                    }),

                    _ => BadRequest(new ProblemDetails
                    {
                        Title = "Bad Request",
                        Detail = result.ErrorMessage ?? "Invalid request data",
                        Status = StatusCodes.Status400BadRequest
                    })
                };
            }

            return Ok();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Registration for {Email} was canceled", request.Email);

            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);

            return Problem(
                title: "Registration Error",
                detail: "An error occurred during registration",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
