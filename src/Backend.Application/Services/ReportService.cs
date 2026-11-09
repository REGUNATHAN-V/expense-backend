using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public class ReportService : IReportService
{
    private const int MaxDays = 365;
    private const int MaxLimit = 20;

    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<PagedResult<MonthlyTotalDto>>> GetMonthlyTotalsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateDates(filter);

        if (error is not null)
        {
            return Result<PagedResult<MonthlyTotalDto>>.Failure(
                error, ResultErrorType.Validation);
        }

        return Result<PagedResult<MonthlyTotalDto>>.Success(
            await _reportRepository.GetMonthlyTotalsAsync(
                filter, cancellationToken));
    }

    public async Task<Result<List<TopCategoryDto>>> GetTopCategoriesAsync(
        int days,
        int limit,
        int? employeeId,
        CancellationToken cancellationToken = default)
    {
        if (days < 1 || days > MaxDays)
        {
            return Result<List<TopCategoryDto>>.Failure(
                $"days must be between 1 and {MaxDays}",
                ResultErrorType.Validation);
        }

        if (limit < 1 || limit > MaxLimit)
        {
            return Result<List<TopCategoryDto>>.Failure(
                $"limit must be between 1 and {MaxLimit}",
                ResultErrorType.Validation);
        }

        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        return Result<List<TopCategoryDto>>.Success(
            await _reportRepository.GetTopCategoriesAsync(
                since, limit, employeeId, cancellationToken));
    }

    public async Task<Result<PagedResult<MismatchedClaimDto>>> GetMismatchedClaimsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateDates(filter);

        if (error is not null)
        {
            return Result<PagedResult<MismatchedClaimDto>>.Failure(
                error, ResultErrorType.Validation);
        }

        return Result<PagedResult<MismatchedClaimDto>>.Success(
            await _reportRepository.GetMismatchedClaimsAsync(
                filter, cancellationToken));
    }

    public async Task<Result<PagedResult<LatestClaimDto>>> GetLatestClaimsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateDates(filter);

        if (error is not null)
        {
            return Result<PagedResult<LatestClaimDto>>.Failure(
                error, ResultErrorType.Validation);
        }

        return Result<PagedResult<LatestClaimDto>>.Success(
            await _reportRepository.GetLatestClaimsAsync(
                filter, cancellationToken));
    }

    public async Task<Result<PagedResult<ExpenseItemDto>>> GetCategoryItemsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var error = ValidateDates(filter);

        if (error is not null)
        {
            return Result<PagedResult<ExpenseItemDto>>.Failure(
                error, ResultErrorType.Validation);
        }

        return Result<PagedResult<ExpenseItemDto>>.Success(
            await _reportRepository.GetCategoryItemsAsync(
                filter, cancellationToken));
    }

    public async Task<Result<List<EmployeeLookupDto>>> GetEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        return Result<List<EmployeeLookupDto>>.Success(
            await _reportRepository.GetEmployeesAsync(cancellationToken));
    }

    private static string? ValidateDates(ReportFilterDto filter)
    {
        if (filter.From is not null &&
            filter.To is not null &&
            filter.From > filter.To)
        {
            return "From date cannot be after To date";
        }

        return null;
    }
}