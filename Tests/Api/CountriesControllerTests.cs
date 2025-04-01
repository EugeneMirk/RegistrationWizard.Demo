using Api.Controllers;
using Core.DTOs.CountryDTOs.Responses;
using Core.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Api;

public class CountriesControllerTests
{
    private readonly Mock<ICountryService> _countryServiceMock = new();
    private readonly Mock<ILogger<CountriesController>> _loggerMock = new();
    private readonly CountriesController _controller;

    public CountriesControllerTests()
    {
        _controller = new CountriesController(_countryServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllCountries_ReturnsCountries_WhenServiceReturnsData()
    {
        var expectedCountries = new List<CountryResponse>
        {
            new(1, "Country A"),
            new(2, "Country B" )
        };

        _countryServiceMock
            .Setup(x => x.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCountries);

        var result = await _controller.GetAllCountries();

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedCountries);
    }

    [Fact]
    public async Task GetAllCountries_Returns500_WhenServiceThrowsException()
    {
        _countryServiceMock
            .Setup(x => x.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetAllCountries();

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetAllCountries_Returns499_WhenOperationCanceled()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _countryServiceMock
            .Setup(x => x.GetAllCountriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var result = await _controller.GetAllCountries(cts.Token);

        result.Result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(499);
    }

    [Fact]
    public async Task GetProvincesByCountryId_ReturnsProvinces_WhenCountryExists()
    {
        const int countryId = 1;
        var expectedProvinces = new List<ProvinceResponse>
        {
            new() { CountryId = countryId, Id =1, Name = "Province A" },
            new() { CountryId = countryId, Id =2, Name = "Province B" }
        };

        _countryServiceMock
            .Setup(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProvinces);

        // Act
        var result = await _controller.GetProvincesByCountryId(countryId);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(expectedProvinces);
    }

    [Fact]
    public async Task GetProvincesByCountryId_Returns404_WhenNoProvincesFound()
    {
        // Arrange
        const int countryId = 99;
        _countryServiceMock
            .Setup(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.GetProvincesByCountryId(countryId);

        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeOfType<ProblemDetails>()
            .Which.Status.Should().Be(404);
    }

    [Fact]
    public async Task GetProvincesByCountryId_Returns500_WhenServiceThrowsException()
    {
        const int countryId = 1;

        _countryServiceMock
            .Setup(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var result = await _controller.GetProvincesByCountryId(countryId);

        result.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetProvincesByCountryId_Returns499_WhenOperationCanceled()
    {
        const int countryId = 1;
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _countryServiceMock
            .Setup(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var result = await _controller.GetProvincesByCountryId(countryId, cts.Token);

        result.Result.Should().BeOfType<StatusCodeResult>()
            .Which.StatusCode.Should().Be(499);
    }
}
