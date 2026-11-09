using Backend.Application.DTOs;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Interfaces.Repositories;

public interface IClaimRepository : IGenericRepository<ClaimEntity>
{
    /// <summary>
    /// Returns the claim with its items only if it belongs to the employee.
    /// A claim owned by someone else comes back as null.
    /// </summary>
    Task<ClaimEntity?> GetOwnedAsync(
        int claimId,
        int employeeId,
        CancellationToken ct = default);

    Task<List<CategoryEntity>> GetCategoriesAsync(
        IReadOnlyCollection<int> categoryIds,
        CancellationToken ct = default);

    Task<List<CategoryEntity>> GetAllCategoriesAsync(
        CancellationToken ct = default);

     Task<List<LeaveTypeEntity>> GetAllLeaveTypesAsync(
        CancellationToken ct = default);

    void RemoveItems(IEnumerable<ClaimItemEntity> items);

    /// <summary>Approver side: any claim with its items (tracked, for updates).</summary>
    Task<ClaimEntity?> GetWithItemsAsync(
        int claimId,
        CancellationToken ct = default);

    /// <summary>
    /// One page of claims with their items, newest first.
    /// employeeId limits the list to one employee (null = everyone).
    /// defaultStatus is used when the filter has no Status (null = all statuses).
    /// </summary>
    Task<PagedResult<ClaimEntity>> GetPagedAsync(
        int? employeeId,
        ClaimStatus? defaultStatus,
        ClaimFilterDto filter,
        CancellationToken ct = default);

    /// <summary>Id -> name for the given users (maker and approver names).</summary>
    Task<Dictionary<int, string>> GetUserNamesAsync(
        IEnumerable<int> userIds,
        CancellationToken ct = default);
}