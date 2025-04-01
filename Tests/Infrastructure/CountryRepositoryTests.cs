using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SQLitePCL;

namespace Tests.Infrastructure;

public class CountryRepositoryTests
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<ILogger<CountryRepository>> _loggerMock = new();

    public CountryRepositoryTests()
    {
        Batteries.Init();

        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new ApplicationDbContext(_options);
        context.Database.EnsureCreated();

        SeedTestData(context);
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        var countries = new List<Country>
            {
                new() { Id = 1, Name = "Country 1", Provinces = new List<Province>
                {
                    new() { Id = 1, Name = "Province 1-1" },
                    new() { Id = 2, Name = "Province 1-2" }
                }},
                new() { Id = 2, Name = "Country 2", Provinces = new List<Province>
                {
                    new() { Id = 3, Name = "Province 2-1" }
                }}
            };

        context.Countries.AddRange(countries);
        context.SaveChanges();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private CountryRepository CreateRepository()
    {
        var context = new ApplicationDbContext(_options);
        return new CountryRepository(context, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCountry_WhenExists()
    {
        var repo = CreateRepository();

        var country = await repo.GetByIdAsync(1);

        Assert.NotNull(country);
        Assert.Equal("Country 1", country.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var repo = CreateRepository();

        var country = await repo.GetByIdAsync(99);

        Assert.Null(country);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCountries()
    {
        var repo = CreateRepository();

        var countries = await repo.GetAllAsync();

        Assert.Equal(2, countries.Count());
        Assert.Contains(countries, c => c.Name == "Country 1");
        Assert.Contains(countries, c => c.Name == "Country 2");
    }

    [Fact]
    public async Task GetProvincesByCountryIdAsync_ReturnsProvinces_WhenCountryExists()
    {
        var repo = CreateRepository();

        var provinces = await repo.GetProvincesByCountryIdAsync(1);

        Assert.Equal(2, provinces.Count());
        Assert.Contains(provinces, p => p.Name == "Province 1-1");
    }

    [Fact]
    public async Task GetProvincesByCountryIdAsync_ReturnsEmpty_WhenCountryHasNoProvinces()
    {
        var repo = CreateRepository();
        var newCountry = new Country { Id = 3, Name = "Country 3" };
        await repo.AddAsync(newCountry);

        var provinces = await repo.GetProvincesByCountryIdAsync(3);

        Assert.Empty(provinces);
    }

    [Fact]
    public async Task AddAsync_SavesCountryToDatabase()
    {
        var repo = CreateRepository();
        var newCountry = new Country { Id = 3, Name = "Country 3" };

        await repo.AddAsync(newCountry);

        var country = await repo.GetByIdAsync(3);
        Assert.NotNull(country);
        Assert.Equal("Country 3", country.Name);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingCountry()
    {
        var repo = CreateRepository();
        var country = await repo.GetByIdAsync(1);
        country.Name = "Updated Country";

        await repo.UpdateAsync(country);

        var updatedCountry = await repo.GetByIdAsync(1);
        Assert.Equal("Updated Country", updatedCountry.Name);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCountryFromDatabase()
    {
        var repo = CreateRepository();
        var country = await repo.GetByIdAsync(1);

        await repo.DeleteAsync(country);

        var deletedCountry = await repo.GetByIdAsync(1);
        Assert.Null(deletedCountry);
    }
}
