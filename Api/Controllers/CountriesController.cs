using Core.DTOs.CountryDTOs.Responses;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Api.Controllers;

/// <summary>
/// API for managing countries and provinces
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CountriesController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly ILogger<CountriesController> _logger;

    public CountriesController(ICountryService countryService, ILogger<CountriesController> logger)
    {
        _countryService = countryService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all available countries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns list of countries</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CountryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CountryResponse>>> GetAllCountries(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching all countries");

            var countries = await _countryService.GetAllCountriesAsync(cancellationToken);

            return Ok(countries);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Countries request was canceled");

            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching countries");

            return Problem(
                title: "Internal Server Error",
                detail: "An error occurred while fetching countries",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Gets provinces for specified country
    /// </summary>
    /// <param name="countryId">Country ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Returns list of provinces</response>
    /// <response code="400">Invalid country ID</response>
    /// <response code="404">Country not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{countryId:int}/provinces")]
    [ProducesResponseType(typeof(IEnumerable<ProvinceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProvinceResponse>>> GetProvincesByCountryId(
        [FromRoute][Range(1, int.MaxValue)] int countryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching provinces for country {CountryId}", countryId);

            var provinces = await _countryService.GetProvincesByCountryIdAsync(countryId, cancellationToken);

            if (provinces == null || !provinces.Any())
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"No provinces found for country ID {countryId}",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(provinces);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Provinces request for country {CountryId} was canceled", countryId);

            return StatusCode(StatusCodes.Status499ClientClosedRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching provinces for country {CountryId}", countryId);

            return Problem(
                title: "Internal Server Error",
                detail: $"An error occurred while fetching provinces for country {countryId}",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
