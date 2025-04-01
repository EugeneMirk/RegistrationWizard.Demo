using Application.Services;
using Core.DTOs.CountryDTOs.Responses;
using Core.Interfaces.Repositories;
using Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application;

public class CountriesServiceTests
{
    private readonly Mock<ICountryRepository> _countryRepositoryMock = new();
    private readonly Mock<IMemoryCache> _cacheMock = new();
    private readonly Mock<ILogger<CountryService>> _loggerMock = new();
    private readonly CountryService _countryService;

    public CountriesServiceTests()
    {
        _countryService = new CountryService(
            _countryRepositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllCountriesAsync_ReturnsCountriesFromCache_WhenCacheExists()
    {
        var cachedCountries = new List<CountryResponse>
        {
            new(new Country { Id = 1, Name = "Cached Country" })
        };

        SetupCacheMock("CountriesCache", cachedCountries);

        var result = await _countryService.GetAllCountriesAsync(CancellationToken.None);

        result.Should().BeEquivalentTo(cachedCountries);
        _countryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllCountriesAsync_FetchesFromRepository_WhenCacheEmpty()
    {
        var dbCountries = new List<Country>
        {
            new() { Id = 1, Name = "Country 1" },
            new() { Id = 2, Name = "Country 2" }
        };

        SetupCacheMock<List<Country>>("CountriesCache", null);
        _countryRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbCountries);

        var result = await _countryService.GetAllCountriesAsync(CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
        _countryRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllCountriesAsync_ReturnsEmptyList_WhenRepositoryReturnsNull()
    {
        SetupCacheMock<List<Country>>("CountriesCache", null);
        _countryRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Country>)null!);

        var result = await _countryService.GetAllCountriesAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllCountriesAsync_ThrowsAndLogsError_WhenRepositoryFails()
    {
        SetupCacheMock<List<Country>>("CountriesCache", null);
        _countryRepositoryMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() =>
            _countryService.GetAllCountriesAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetProvincesByCountryIdAsync_ReturnsProvincesFromCache_WhenCacheExists()
    {
        // Arrange
        const int countryId = 1;
        var cacheKey = $"ProvincesForCountry_{countryId}";
        var cachedProvinces = new List<ProvinceResponse>
        {
            new(new Province { Id = 1, Name = "Cached Province" })
        };

        SetupCacheMock(cacheKey, cachedProvinces);

        var result = await _countryService.GetProvincesByCountryIdAsync(countryId, CancellationToken.None);

        result.Should().BeEquivalentTo(cachedProvinces);
        _countryRepositoryMock.Verify(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetProvincesByCountryIdAsync_FetchesFromRepository_WhenCacheEmpty()
    {
        // Arrange
        const int countryId = 1;
        var cacheKey = $"ProvincesForCountry_{countryId}";
        var dbProvinces = new List<Province>
        {
            new() { Id = 1, Name = "Province 1", CountryId = countryId },
            new() { Id = 2, Name = "Province 2", CountryId = countryId }
        };

        SetupCacheMock<List<Province>>(cacheKey, null);

        _countryRepositoryMock.Setup(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dbProvinces);
        var result = await _countryService.GetProvincesByCountryIdAsync(countryId, CancellationToken.None);

        result.Should().HaveCount(2);
        result.First().Id.Should().Be(1);
        _countryRepositoryMock.Verify(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProvincesByCountryIdAsync_ThrowsAndLogsError_WhenRepositoryFails()
    {
        const int countryId = 1;
        var cacheKey = $"ProvincesForCountry_{countryId}";

        SetupCacheMock<List<Province>>(cacheKey, null);
        _countryRepositoryMock.Setup(x => x.GetProvincesByCountryIdAsync(countryId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        await Assert.ThrowsAsync<Exception>(() =>
            _countryService.GetProvincesByCountryIdAsync(countryId, CancellationToken.None));
    }

    private void SetupCacheMock<T>(string cacheKey, T? cachedValue)
    {
        object? outValue = cachedValue;

        _cacheMock.Setup(x => x.TryGetValue(cacheKey, out outValue))
            .Returns(cachedValue != null);

        _cacheMock.Setup(x => x.CreateEntry(It.IsAny<string>()))
            .Returns(Mock.Of<ICacheEntry>);
    }
}
