
using Microsoft.EntityFrameworkCore;
using Backend.Application.Interfaces.Repositories;
using Backend.Domain.Entities;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class AuthRepository
    : GenericRepository<UserEntity>, IAuthRepository
{
    public AuthRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<UserEntity?> GetByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        return await DbSet.FirstOrDefaultAsync(
            user => user.Email == email && !user.IsDeleted,
            ct);
    }
}
