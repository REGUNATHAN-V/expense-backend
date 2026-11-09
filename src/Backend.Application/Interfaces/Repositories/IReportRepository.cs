using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<PagedResult<MonthlyTotalDto>> GetMonthlyTotalsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default);

    /// <summary>Highest-spend categories since a date.</summary>
    Task<List<TopCategoryDto>> GetTopCategoriesAsync(
        DateOnly since,
        int limit,
        int? employeeId,
        CancellationToken ct = default);

    /// <summary>Claims whose TotalAmount differs from the sum of their items.</summary>
    Task<PagedResult<MismatchedClaimDto>> GetMismatchedClaimsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default);

    /// <summary>Each employee's most recent claim.</summary>
    Task<PagedResult<LatestClaimDto>> GetLatestClaimsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default);

    /// <summary>The expense items behind a monthly total, newest first.</summary>
    Task<PagedResult<ExpenseItemDto>> GetCategoryItemsAsync(
        ReportFilterDto filter,
        CancellationToken ct = default);

    /// <summary>Employees for the filter pickers.</summary>
    Task<List<EmployeeLookupDto>> GetEmployeesAsync(
        CancellationToken ct = default);
}