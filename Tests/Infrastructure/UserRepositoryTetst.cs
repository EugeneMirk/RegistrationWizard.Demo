using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SQLitePCL;

namespace Tests.Infrastructure;

public class UserRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly Mock<ILogger<UserRepository>> _loggerMock = new();
    private readonly Country _testCountry = new() { Id = 1, Name = "Test Country" };
    private readonly Province _testProvince = new() { Id = 1, Name = "Test Province", CountryId = 1 };

    public UserRepositoryTests()
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
        context.Countries.Add(_testCountry);
        context.Provinces.Add(_testProvince);

        context.Users.AddRange(
            new User
            {
                Id = 1,
                Email = "user1@test.com",
                Country = _testCountry,
                Province = _testProvince,
                PasswordHash = "PHash1"
            },
            new User
            {
                Id = 2,
                Email = "user2@test.com",
                Country = _testCountry,
                Province = _testProvince,
                PasswordHash = "PaHash2"
            }
        );

        context.SaveChanges();
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    private UserRepository CreateRepository()
    {
        var context = new ApplicationDbContext(_options);
        return new UserRepository(context, _loggerMock.Object);
    }

    [Fact]
    public async Task AddAsync_SavesUserToDatabase()
    {
        var repo = CreateRepository();
        var newUser = new User
        {
            Email = "new@test.com",
            CountryId = 1,
            ProvinceId = 1,
            PasswordHash = "PHash3"
        };

        await repo.AddAsync(newUser);

        var user = await repo.GetByIdAsync(newUser.Id);
        Assert.NotNull(user);
        Assert.Equal("new@test.com", user.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsUserWithNavigationProperties()
    {
        var repo = CreateRepository();

        var user = await repo.GetByIdAsync(1);

        Assert.NotNull(user);
        Assert.Equal("user1@test.com", user.Email);
        Assert.NotNull(user.Country);
        Assert.NotNull(user.Province);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenUserNotFound()
    {
        var repo = CreateRepository();

        var user = await repo.GetByIdAsync(99);

        Assert.Null(user);
    }

    [Fact]
    public async Task GetByEmailAsync_ReturnsCorrectUser()
    {
        var repo = CreateRepository();

        var user = await repo.GetByEmailAsync("user2@test.com");

        Assert.NotNull(user);
        Assert.Equal(2, user.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsersWithNavigationProperties()
    {
        var repo = CreateRepository();

        var users = await repo.GetAllAsync();

        Assert.Equal(2, users.Count());
        Assert.All(users, u =>
        {
            Assert.NotNull(u.Country);
            Assert.NotNull(u.Province);
        });
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingUser()
    {
        var repo = CreateRepository();
        var user = await repo.GetByIdAsync(1);
        user.Email = "updated@test.com";

        await repo.UpdateAsync(user);

        var updatedUser = await repo.GetByIdAsync(1);
        Assert.Equal("updated@test.com", updatedUser.Email);
    }

    [Fact]
    public async Task DeleteAsync_RemovesUserFromDatabase()
    {
        var repo = CreateRepository();
        var user = await repo.GetByIdAsync(1);

        await repo.DeleteAsync(user);

        var deletedUser = await repo.GetByIdAsync(1);
        Assert.Null(deletedUser);
    }

    [Theory]
    [InlineData("user1@test.com", true)]
    [InlineData("nonexistent@test.com", false)]
    public async Task IsEmailExistsAsync_ReturnsCorrectResult(string email, bool expected)
    {
        var repo = CreateRepository();

        var result = await repo.IsEmailExistsAsync(email);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AddAsync_Throws_WhenEmailAlreadyExists()
    {
        var repo = CreateRepository();

        var newUser = new User
        {
            Email = "user1@test.com",
            PasswordHash = "PHash4"
        };

        await Assert.ThrowsAsync<DbUpdateException>(() => repo.AddAsync(newUser));
    }
}