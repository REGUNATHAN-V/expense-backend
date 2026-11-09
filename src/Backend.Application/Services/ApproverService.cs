using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class ApproverService : IApproverService
{
    private readonly IClaimRepository _claimRepository;

    public ApproverService(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<Result<PagedResult<ApproverClaimDto>>> GetClaimsAsync(
        ClaimFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var page = await _claimRepository.GetPagedAsync(
            filter.EmployeeId,
            ClaimStatus.Submitted,
            filter,
            cancellationToken);

        var names = await LoadNamesAsync(page.Items, cancellationToken);

        return Result<PagedResult<ApproverClaimDto>>.Success(
            page.Map(claim => ToResponse(claim, names)));
    }

    public async Task<Result<ApproverClaimDto>> GetClaimAsync(
        int claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetWithItemsAsync(
            claimId, cancellationToken);

        if (claim is null)
        {
            return Result<ApproverClaimDto>.Failure(
                "Claim not found",
                ResultErrorType.NotFound);
        }

        var names = await LoadNamesAsync(new[] { claim }, cancellationToken);

        return Result<ApproverClaimDto>.Success(ToResponse(claim, names));
    }

    public Task<Result<ApproverClaimDto>> ApproveAsync(
        int approverId,
        int claimId,
        CancellationToken cancellationToken = default)
        => DecideAsync(approverId, claimId, ClaimStatus.Approved, cancellationToken);

    public Task<Result<ApproverClaimDto>> RejectAsync(
        int approverId,
        int claimId,
        CancellationToken cancellationToken = default)
        => DecideAsync(approverId, claimId, ClaimStatus.Rejected, cancellationToken);

    // Sends the claim back to the maker, who can edit and resubmit it.
    public Task<Result<ApproverClaimDto>> ReturnAsync(
        int approverId,
        int claimId,
        CancellationToken cancellationToken = default)
        => DecideAsync(approverId, claimId, ClaimStatus.Returned, cancellationToken);

    private async Task<Result<ApproverClaimDto>> DecideAsync(
        int approverId,
        int claimId,
        ClaimStatus newStatus,
        CancellationToken cancellationToken)
    {
        var claim = await _claimRepository.GetWithItemsAsync(
            claimId, cancellationToken);

        if (claim is null)
        {
            return Result<ApproverClaimDto>.Failure(
                "Claim not found",
                ResultErrorType.NotFound);
        }

        if (claim.EmployeeId == approverId)
        {
            return Result<ApproverClaimDto>.Failure(
                "You cannot decide your own claim",
                ResultErrorType.Forbidden);
        }

        if (claim.Status != ClaimStatus.Submitted)
        {
            return Result<ApproverClaimDto>.Failure(
                "Only submitted claims can be decided",
                ResultErrorType.Conflict);
        }

        claim.Status = newStatus;
        claim.ApproverId = approverId;
        claim.DecidedAt = DateTime.UtcNow;

        await _claimRepository.SaveChangesAsync(cancellationToken);

        var names = await LoadNamesAsync(new[] { claim }, cancellationToken);

        return Result<ApproverClaimDto>.Success(ToResponse(claim, names));
    }

    // One query for every maker and approver name on the page.
    private Task<Dictionary<int, string>> LoadNamesAsync(
        IEnumerable<ClaimEntity> claims,
        CancellationToken cancellationToken)
    {
        var list = claims.ToList();

        var ids = list.Select(c => c.EmployeeId)
            .Concat(list
                .Where(c => c.ApproverId != null)
                .Select(c => c.ApproverId!.Value));

        return _claimRepository.GetUserNamesAsync(ids, cancellationToken);
    }

    private static ApproverClaimDto ToResponse(
        ClaimEntity claim,
        Dictionary<int, string> names)
    {
        string? approverName = null;

        if (claim.ApproverId is not null &&
            names.TryGetValue(claim.ApproverId.Value, out var approver))
        {
            approverName = approver;
        }

        return new ApproverClaimDto
        {
            Id = claim.Id,
            EmployeeId = claim.EmployeeId,
            EmployeeName = names.TryGetValue(claim.EmployeeId, out var maker)
                ? maker
                : string.Empty,
            ClaimDate = claim.ClaimDate,
            TotalAmount = claim.TotalAmount,
            Status = claim.Status.ToString(),
            ApproverId = claim.ApproverId,
            ApproverName = approverName,
            DecidedAt = claim.DecidedAt,
            Items = claim.Items
                .Select(i => new ClaimItemResponseDto
                {
                    Id = i.Id,
                    CategoryId = i.CategoryId,
                    Amount = i.Amount,
                    ExpenseDate = i.ExpenseDate,
                    Description = i.Description,
                    ReceiptUrl = i.ReceiptUrl
                })
                .ToList()
        };
    }
}