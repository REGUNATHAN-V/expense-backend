using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Helpers;
using Backend.API.Common.Responses;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.API.Controllers;

[ApiController]
[Route("admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IReportService _reportService;
    private readonly IApproverService _approverService;

    public AdminController(
        IAdminService adminService,
        IReportService reportService,
        IApproverService approverService)
    {
        _adminService = adminService;
        _reportService = reportService;
        _approverService = approverService;
    }

    // ---------- Claims (read-only) ----------

    // Full details of any claim, so the admin can open it from the reports.
    // GET /admin/claims/5
    [HttpGet("claims/{id:int}")]
    public async Task<IActionResult> GetClaim(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _approverService.GetClaimAsync(
            id, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<ApproverClaimDto>.SuccessResponse(
                data: result.Data!));
    }

    // ---------- Users ----------

    // GET /admin/users?index=1&limit=20
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] PagedRequestDto paging,
        CancellationToken cancellationToken)
    {
        return this.ToPagedResponse(
            await _adminService.GetUsersAsync(paging, cancellationToken));
    }

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateRole(
        int id,
        [FromBody] UpdateRoleDto dto,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var adminId))
        {
            return InvalidToken();
        }

        var result = await _adminService.UpdateRoleAsync(
            adminId, id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<UserResponseDto>.SuccessResponse(
                message: "Role updated",
                data: result.Data!));
    }

    [HttpPut("users/{id:int}/active")]
    public async Task<IActionResult> SetActive(
        int id,
        [FromBody] SetActiveDto dto,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var adminId))
        {
            return InvalidToken();
        }

        var result = await _adminService.SetActiveAsync(
            adminId, id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<UserResponseDto>.SuccessResponse(
                message: dto.IsActive ? "User activated" : "User deactivated",
                data: result.Data!));
    }

    // ---------- Reports ----------

    // Total per employee per month, by category (employees with no claims included).
    // GET /admin/reports/monthly-totals?from=2026-01-01&to=2026-12-31&employeeId=2&categoryId=1&status=Approved&index=1&limit=20
    [HttpGet("reports/monthly-totals")]
    public async Task<IActionResult> MonthlyTotals(
        [FromQuery] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        return this.ToPagedResponse(
            await _reportService.GetMonthlyTotalsAsync(
                filter, cancellationToken));
    }

    // Top categories by amount. Default: last 90 days, top 3. Not paged.
    // GET /admin/reports/top-categories?days=90&limit=3&employeeId=2
    [HttpGet("reports/top-categories")]
    public async Task<IActionResult> TopCategories(
        [FromQuery] int days = 90,
        [FromQuery] int limit = 3,
        [FromQuery] int? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetTopCategoriesAsync(
            days, limit, employeeId, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<List<TopCategoryDto>>.SuccessResponse(
                data: result.Data!));
    }

    // Claims where TotalAmount does not match the sum of their items.
    // GET /admin/reports/mismatched-claims?employeeId=2&status=Submitted&index=1&limit=20
    [HttpGet("reports/mismatched-claims")]
    public async Task<IActionResult> MismatchedClaims(
        [FromQuery] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        return this.ToPagedResponse(
            await _reportService.GetMismatchedClaimsAsync(
                filter, cancellationToken));
    }

    // Each employee's latest claim.
    // GET /admin/reports/latest-claims?status=Approved&index=1&limit=20
    [HttpGet("reports/latest-claims")]
    public async Task<IActionResult> LatestClaims(
        [FromQuery] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        return this.ToPagedResponse(
            await _reportService.GetLatestClaimsAsync(
                filter, cancellationToken));
    }

    // Every expense item behind a monthly total, with its claim, maker and approver.
    // GET /admin/reports/items?employeeId=2&categoryId=1&from=2026-11-01&to=2026-11-30&status=Approved&index=1&limit=20
    [HttpGet("reports/items")]
    public async Task<IActionResult> CategoryItems(
        [FromQuery] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        return this.ToPagedResponse(
            await _reportService.GetCategoryItemsAsync(
                filter, cancellationToken));
    }

    private IActionResult InvalidToken()
    {
        return Unauthorized(
            ApiErrorResponse.FailureResponse("Invalid token"));
    }
}