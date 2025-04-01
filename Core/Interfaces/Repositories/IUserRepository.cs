using Domain.Entities;

namespace Core.Interfaces.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailExistsAsync(string email, CancellationToken token);
}
