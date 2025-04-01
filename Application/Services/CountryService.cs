using Core.DTOs.CountryDTOs.Responses;
using Core.Interfaces.Repositories;
using Core.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class CountryService : ICountryService
{
    private const string CountriesCacheKey = "CountriesCache";
    private const string ProvincesCachePrefix = "ProvincesForCountry_";

    private readonly ICountryRepository _countryRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CountryService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

    public CountryService(
        ICountryRepository countryRepository,
        IMemoryCache cache,
        ILogger<CountryService> logger)
    {
        _countryRepository = countryRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IEnumerable<CountryResponse>> GetAllCountriesAsync(CancellationToken token)
    {
        try
        {
            return await _cache.GetOrCreateAsync(CountriesCacheKey, async entry =>
            {
                var countries = await _countryRepository.GetAllAsync(token);

                // Если countries null или пустой, не кэшируем и возвращаем пустую коллекцию
                if (countries == null || !countries.Any())
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.Zero; // Не кэшируем
                    return new List<CountryResponse>();
                }

                entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;
                return countries.Select(c => new CountryResponse(c)).ToList();
            }) ?? []; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching countries");
            throw;
        }
    }

    public async Task<IEnumerable<ProvinceResponse>> GetProvincesByCountryIdAsync(int countryId, CancellationToken token)
    {
        var cacheKey = $"{ProvincesCachePrefix}{countryId}";

        try
        {
            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                var provinces = await _countryRepository.GetProvincesByCountryIdAsync(countryId, token);

                if (provinces == null || !provinces.Any())
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.Zero;
                    return [];
                }

                entry.AbsoluteExpirationRelativeToNow = _cacheExpiration;

                return provinces.Select(p => new ProvinceResponse(p)).ToList();
            }) ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching provinces for country {CountryId}", countryId);
            throw;
        }
    }
}
