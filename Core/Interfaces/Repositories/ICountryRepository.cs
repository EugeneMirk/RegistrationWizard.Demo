using Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface ICountryRepository : IRepository<Country>
{
    Task<IEnumerable<Province>> GetProvincesByCountryIdAsync(int id, CancellationToken token);
}
