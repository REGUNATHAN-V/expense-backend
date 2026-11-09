
using Backend.Domain.Entities;

namespace Backend.Application.Interfaces.Repositories;

public interface IAuthRepository : IGenericRepository<UserEntity>
{
    Task<UserEntity?> GetByEmailAsync(
        string email,
        CancellationToken ct = default);
}
