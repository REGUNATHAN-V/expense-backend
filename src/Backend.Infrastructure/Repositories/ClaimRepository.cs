using Microsoft.EntityFrameworkCore;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Domain.Entities;
using Backend.Domain.Enums;
using Backend.Infrastructure.Common;
using Backend.Infrastructure.Persistence;

namespace Backend.Infrastructure.Repositories;

public class ClaimRepository
    : GenericRepository<ClaimEntity>, IClaimRepository
{
    public ClaimRepository(AppDbContext context)
        : base(context)
    {
    }

    public async Task<ClaimEntity?> GetOwnedAsync(
        int claimId,
        int employeeId,
        CancellationToken ct = default)
    {
        // Filtering on EmployeeId here means a maker can never load
        // another maker's claim, even by guessing the id.
        return await DbSet
            .Include(claim => claim.Items)
            .FirstOrDefaultAsync(
                claim => claim.Id == claimId &&
                         claim.EmployeeId == employeeId &&
                         !claim.IsDeleted,
                ct);
    }

    public async Task<List<CategoryEntity>> GetCategoriesAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken ct = default)
    {
        return await Context.Set<CategoryEntity>()
            .AsNoTracking()
            .Where(category => categoryIds.Contains(category.Id) &&
                               !category.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<List<CategoryEntity>> GetAllCategoriesAsync(
        CancellationToken ct = default)
    {
        return await Context.Set<CategoryEntity>()
            .AsNoTracking()
            .Where(category => !category.IsDeleted)
            .OrderBy(category => category.Name)
            .ToListAsync(ct);
    }
    public async Task<List<LeaveTypeEntity>> GetAllLeaveTypesAsync(
        CancellationToken ct = default)
    {
        return await Context.Set<LeaveTypeEntity>()
            .AsNoTracking()
            .ToListAsync(ct);
    }

    

    public void RemoveItems(IEnumerable<ClaimItemEntity> items)
    {
        Context.Set<ClaimItemEntity>().RemoveRange(items);
    }

    public async Task<ClaimEntity?> GetWithItemsAsync(
        int claimId,
        CancellationToken ct = default)
    {
        return await DbSet
            .Include(claim => claim.Items)
            .FirstOrDefaultAsync(
                claim => claim.Id == claimId && !claim.IsDeleted,
                ct);
    }

    public async Task<PagedResult<ClaimEntity>> GetPagedAsync(
        int? employeeId,
        ClaimStatus? defaultStatus,
        ClaimFilterDto filter,
        CancellationToken ct = default)
    {
        var query = DbSet
            .AsNoTracking()
            .Include(claim => claim.Items)
            .Where(claim => !claim.IsDeleted);

        if (employeeId is not null)
        {
            var id = employeeId.Value;
            query = query.Where(claim => claim.EmployeeId == id);
        }

        var status = filter.Status ?? defaultStatus;

        if (status is not null)
        {
            var s = status.Value;
            query = query.Where(claim => claim.Status == s);
        }

        // Only claims that contain at least one item of this category.
        if (filter.CategoryId is not null)
        {
            var categoryId = filter.CategoryId.Value;
            query = query.Where(claim => claim.Items.Any(
                item => item.CategoryId == categoryId && !item.IsDeleted));
        }

        if (filter.From is not null)
        {
            var from = filter.From.Value;
            query = query.Where(claim => claim.ClaimDate >= from);
        }

        if (filter.To is not null)
        {
            var to = filter.To.Value;
            query = query.Where(claim => claim.ClaimDate <= to);
        }

        return await query
            .OrderByDescending(claim => claim.ClaimDate)
            .ThenByDescending(claim => claim.Id)
            .ToPagedAsync(filter.Index, filter.Limit, ct);
    }

    public async Task<Dictionary<int, string>> GetUserNamesAsync(
        IEnumerable<int> userIds,
        CancellationToken ct = default)
    {
        var ids = userIds.Distinct().ToList();

        if (ids.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        return await Context.Set<UserEntity>()
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.Name, ct);
    }
}