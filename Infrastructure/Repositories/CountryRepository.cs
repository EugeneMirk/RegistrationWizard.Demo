using Core.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Infrastructure.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CountryRepository> _logger;

    public CountryRepository(
        ApplicationDbContext context,
        ILogger<CountryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Country?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Countries
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting country by ID {Id}", id);

            throw;
        }
    }

    public async Task<IEnumerable<Country>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Countries
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all countries");
            throw;
        }
    }

    public async Task<IEnumerable<Province>> GetProvincesByCountryIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Countries
                .Where(c => c.Id == id)
                .SelectMany(c => c.Provinces)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provinces for country {Id}", id);

            throw;
        }
    }

    public async Task AddAsync(Country entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Countries.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding country");

            throw;
        }
    }

    public async Task UpdateAsync(Country entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Countries.Update(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating country {Id}", entity.Id);

            throw;
        }
    }

    public async Task DeleteAsync(Country entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Countries.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting country {Id}", entity.Id);

            throw;
        }
    }
}
