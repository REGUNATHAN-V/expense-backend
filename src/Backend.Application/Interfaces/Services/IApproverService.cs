using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IApproverService
{
    /// <summary>Claims list. Status defaults to Submitted when not given.</summary>
    Task<Result<PagedResult<ApproverClaimDto>>> GetClaimsAsync(
        ClaimFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<ApproverClaimDto>> GetClaimAsync(
        int claimId,
        CancellationToken cancellationToken = default);

    Task<Result<ApproverClaimDto>> ApproveAsync(
        int approverId,
        int claimId,
        CancellationToken cancellationToken = default);

    Task<Result<ApproverClaimDto>> RejectAsync(
        int approverId,
        int claimId,
        CancellationToken cancellationToken = default);

    Task<Result<ApproverClaimDto>> ReturnAsync(
        int approverId,
        int claimId,
        CancellationToken cancellationToken = default);
}