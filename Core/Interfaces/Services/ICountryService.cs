using Core.DTOs.CountryDTOs.Responses;

namespace Core.Interfaces.Services;

public interface ICountryService
{
    Task<IEnumerable<CountryResponse>> GetAllCountriesAsync(CancellationToken token);

    Task<IEnumerable<ProvinceResponse>> GetProvincesByCountryIdAsync(int counrtyId, CancellationToken token);
}
