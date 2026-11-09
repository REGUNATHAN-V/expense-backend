using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IReportService
{
    Task<Result<PagedResult<MonthlyTotalDto>>> GetMonthlyTotalsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<List<TopCategoryDto>>> GetTopCategoriesAsync(
        int days,
        int limit,
        int? employeeId,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<MismatchedClaimDto>>> GetMismatchedClaimsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<LatestClaimDto>>> GetLatestClaimsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<PagedResult<ExpenseItemDto>>> GetCategoryItemsAsync(
        ReportFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<Result<List<EmployeeLookupDto>>> GetEmployeesAsync(
        CancellationToken cancellationToken = default);
}