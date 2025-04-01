namespace Core.Interfaces.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken token);

    Task<IEnumerable<T>> GetAllAsync(CancellationToken token);

    Task AddAsync(T entity, CancellationToken token);

    Task UpdateAsync(T entity, CancellationToken token);

    Task DeleteAsync(T entity, CancellationToken token);
}
