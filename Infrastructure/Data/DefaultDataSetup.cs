using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data;

public class DefaultDataSetup
{
    public static async Task Initialize(ApplicationDbContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger<DefaultDataSetup>();

        try
        {
            if (await context.Countries.AnyAsync())
            {              
                return;
            }

            logger.LogInformation("Seeding database with default data...");

            // Create exactly 2 test countries as per requirements
            var countries = new List<Country>
            {
                new Country { Name = "Country 1" },
                new Country { Name = "Country 2" },
                new Country { Name = "Country 2" },
            };

            countries[0].Provinces = new List<Province>
            {
                new Province { Name = "Province 1.1" },
                new Province { Name = "Province 1.2" },
                new Province { Name = "Province 1.3" },
                new Province { Name = "Province 1.4" },
            };

            countries[1].Provinces = new List<Province>
            {
                new Province { Name = "Province 2.1" },
                new Province { Name = "Province 2.2" },
                new Province { Name = "Province 2.3" },
            };

            countries[2].Provinces = new List<Province>
            {
                new Province { Name = "Province 3.1" },
                new Province { Name = "Province 3.2" },
                new Province { Name = "Province 3.3" },
                new Province { Name = "Province 3.4" },
                new Province { Name = "Province 3.5" },
                new Province { Name = "Province 3.6" },
            };

            await context.Countries.AddRangeAsync(countries);

            var users = new List<User>
            {
                new User
                {
                    Email = "user1@example.com",
                    PasswordHash = "hashed_password1",
                    Country = countries[0],
                    Province = countries[0].Provinces.ElementAt(0)
                },
                new User
                {
                    Email = "user2@example.com",
                    PasswordHash = "hashed_password2",
                    Country = countries[0],
                    Province = countries[0].Provinces.ElementAt(1)
                },
                // Additional users...
                new User
                {
                    Email = "user3@example.com",
                    PasswordHash = "hashed_password3",
                    Country = countries[1],
                    Province = countries[1].Provinces.ElementAt(0)
                },
                new User
                {
                    Email = "user4@example.com",
                    PasswordHash = "hashed_password4",
                    Country = countries[1],
                    Province = countries[1].Provinces.ElementAt(1)
                },
                new User
                {
                    Email = "user5@example.com",
                    PasswordHash = "hashed_password5",
                    Country = countries[2],
                    Province = countries[2].Provinces.ElementAt(2)
                }
            };

            await context.Users.AddRangeAsync(users);
            await context.SaveChangesAsync();

            logger.LogInformation("Test data seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding test data");
            throw;
        }
    }
}
