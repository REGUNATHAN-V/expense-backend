using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IMakerService
{
    Task<Result<ClaimResponseDto>> CreateAsync(
        int employeeId,
        CreateClaimDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<ClaimResponseDto>> UpdateAsync(
        int employeeId,
        int claimId,
        UpdateClaimDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<ClaimResponseDto>> CancelAsync(
        int employeeId,
        int claimId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ClaimResponseDto>>> GetMyClaimsAsync(
        int employeeId,
        ClaimFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<ClaimResponseDto>> GetMyClaimAsync(
        int employeeId,
        int claimId,
        CancellationToken cancellationToken = default);
}