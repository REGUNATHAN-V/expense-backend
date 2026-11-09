using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class MakerService : IMakerService
{
    private const int MaxItemsPerClaim = 50;

    private readonly IClaimRepository _claimRepository;

    public MakerService(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<Result<ClaimResponseDto>> CreateAsync(
        int employeeId,
        CreateClaimDto dto,
        CancellationToken cancellationToken = default)
    {
        var error = await ValidateAsync(dto, cancellationToken);

        if (error is not null)
        {
            return Result<ClaimResponseDto>.Failure(
                error,
                ResultErrorType.Validation);
        }

        var claim = new ClaimEntity
        {
            // The owner always comes from the token, never from the body.
            EmployeeId = employeeId,
            ClaimDate = dto.ClaimDate,
            Signature = dto.Signature,

            
            Status = dto.Submit
                ? ClaimStatus.Submitted
                : ClaimStatus.Draft,
            Items = dto.Items.Select(ToEntity).ToList()
        };

        claim.TotalAmount = claim.Items.Sum(i => i.Amount);

        // One SaveChanges writes the claim and all its items in a
        // single database transaction.
        await _claimRepository.AddAsync(claim, cancellationToken);
        await _claimRepository.SaveChangesAsync(cancellationToken);

        return Result<ClaimResponseDto>.Success(ToResponse(claim));
    }

    public async Task<Result<ClaimResponseDto>> UpdateAsync(
        int employeeId,
        int claimId,
        UpdateClaimDto dto,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetOwnedAsync(
            claimId, employeeId, cancellationToken);

        if (claim is null)
        {
            return Result<ClaimResponseDto>.Failure(
                "Claim not found",
                ResultErrorType.NotFound);
        }

        if (claim.Status is not (ClaimStatus.Draft or ClaimStatus.Returned))
        {
            return Result<ClaimResponseDto>.Failure(
                "Only draft or returned claims can be edited",
                ResultErrorType.Conflict);
        }

        var error = await ValidateAsync(dto, cancellationToken);

        if (error is not null)
        {
            return Result<ClaimResponseDto>.Failure(
                error,
                ResultErrorType.Validation);
        }

        _claimRepository.RemoveItems(claim.Items.ToList());

        claim.ClaimDate = dto.ClaimDate;
        claim.Signature = dto.Signature;
        claim.Items = dto.Items.Select(ToEntity).ToList();
        claim.TotalAmount = claim.Items.Sum(i => i.Amount);

        if (dto.Submit)
        {
            claim.Status = ClaimStatus.Submitted;
            claim.ApproverId = null;
            claim.DecidedAt = null;
        }

        await _claimRepository.SaveChangesAsync(cancellationToken);

        return Result<ClaimResponseDto>.Success(ToResponse(claim));
    }

    public async Task<Result<ClaimResponseDto>> CancelAsync(
        int employeeId,
        int claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetOwnedAsync(
            claimId, employeeId, cancellationToken);

        if (claim is null)
        {
            return Result<ClaimResponseDto>.Failure(
                "Claim not found",
                ResultErrorType.NotFound);
        }

        // A claim can be withdrawn until an approver has decided on it.
        if (claim.Status is not (ClaimStatus.Draft or ClaimStatus.Submitted))
        {
            return Result<ClaimResponseDto>.Failure(
                "Only draft or submitted claims can be cancelled",
                ResultErrorType.Conflict);
        }

        claim.Status = ClaimStatus.Cancelled;

        await _claimRepository.SaveChangesAsync(cancellationToken);

        return Result<ClaimResponseDto>.Success(ToResponse(claim));
    }

    public async Task<Result<PagedResult<ClaimResponseDto>>> GetMyClaimsAsync(
        int employeeId,
        ClaimFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        // The owner always comes from the token, so filter.EmployeeId is ignored.
        var page = await _claimRepository.GetPagedAsync(
            employeeId, null, filter, cancellationToken);

        return Result<PagedResult<ClaimResponseDto>>.Success(
            page.Map(claim => ToResponse(claim)));
    }

    public async Task<Result<ClaimResponseDto>> GetMyClaimAsync(
        int employeeId,
        int claimId,
        CancellationToken cancellationToken = default)
    {
        var claim = await _claimRepository.GetOwnedAsync(
            claimId, employeeId, cancellationToken);

        if (claim is null)
        {
            return Result<ClaimResponseDto>.Failure(
                "Claim not found",
                ResultErrorType.NotFound);
        }

        return Result<ClaimResponseDto>.Success(ToResponse(claim));
    }

    // One day of slack so a user ahead of UTC (such as IST in the
    // early morning) is not rejected for "today".
    private static DateOnly LatestAllowedDate()
    {
        return DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
    }

    private async Task<string?> ValidateAsync(
        SaveClaimDto dto,
        CancellationToken cancellationToken)
    {
        if (dto.Items.Count == 0)
        {
            return "A claim needs at least one item";
        }

        if (dto.Items.Count > MaxItemsPerClaim)
        {
            return $"A claim can have at most {MaxItemsPerClaim} items";
        }

        var latest = LatestAllowedDate();

        if (dto.ClaimDate > latest)
        {
            return "Claim date cannot be in the future";
        }

        if (dto.Items.Any(i => i.ExpenseDate > latest))
        {
            return "Expense date cannot be in the future";
        }

        var categoryIds = dto.Items
            .Select(i => i.CategoryId)
            .Distinct()
            .ToList();

        var categories = await _claimRepository.GetCategoriesAsync(
            categoryIds, cancellationToken);

        var missing = categoryIds
            .Except(categories.Select(c => c.Id))
            .ToList();

        if (missing.Count > 0)
        {
            return $"Unknown category id: {string.Join(", ", missing)}";
        }

        foreach (var item in dto.Items)
        {
            var category = categories.First(c => c.Id == item.CategoryId);

            if (category.MaxPerItem is not null &&
                item.Amount > category.MaxPerItem)
            {
                return $"{category.Name} items are limited to " +
                       $"{category.MaxPerItem:0.00} each";
            }
        }

        return null;
    }

    private static ClaimItemEntity ToEntity(ClaimItemDto dto)
    {
        return new ClaimItemEntity
        {
            CategoryId = dto.CategoryId,
            Amount = Math.Round(dto.Amount, 2),
            ExpenseDate = dto.ExpenseDate,
            Description = dto.Description?.Trim(),
            ReceiptUrl = dto.ReceiptUrl?.Trim()
        };
    }

    private static ClaimResponseDto ToResponse(ClaimEntity claim)
    {
        return new ClaimResponseDto
        {
            Id = claim.Id,
            ClaimDate = claim.ClaimDate,
            TotalAmount = claim.TotalAmount,
            Status = claim.Status.ToString(),
            Signature = claim.Signature,
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