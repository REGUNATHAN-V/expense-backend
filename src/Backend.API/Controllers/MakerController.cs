using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Helpers;
using Backend.API.Common.Responses;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.API.Controllers;

[ApiController]
[Route("maker/claims")]
[Authorize(Roles = "Maker")]
public class MakerController : ControllerBase
{
    private readonly IMakerService _makerService;
    private readonly IReportService _reportService;

    public MakerController(
        IMakerService makerService,
        IReportService reportService)
    {
        _makerService = makerService;
        _reportService = reportService;
    }

    // My claims, newest first, so I can see which are approved.
    // GET /maker/claims?status=Approved&from=2026-11-01&to=2026-11-30&index=1&limit=20
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] ClaimFilterDto filter,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        return this.ToPagedResponse(
            await _makerService.GetMyClaimsAsync(
                employeeId, filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOne(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        var result = await _makerService.GetMyClaimAsync(
            employeeId, id, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this,
                result.Error,
                result.ErrorType);
        }

        return Ok(
            ApiResponse<ClaimResponseDto>.SuccessResponse(
                data: result.Data!));
    }

    // My own monthly totals by category. The employee always comes from the token.
    // GET /maker/claims/monthly-totals?from=2026-01-01&to=2026-12-31&categoryId=1&status=Approved
    [HttpGet("monthly-totals")]
    public async Task<IActionResult> MyMonthlyTotals(
        [FromQuery] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        filter.EmployeeId = employeeId;

        return this.ToPagedResponse(
            await _reportService.GetMonthlyTotalsAsync(
                filter, cancellationToken));
    }

    // The items behind one of my monthly totals. The employee comes from the token.
    // GET /maker/claims/items?categoryId=1&from=2026-11-01&to=2026-11-30&status=Approved
    [HttpGet("items")]
    public async Task<IActionResult> MyItems(
        [FromQuery] ReportFilterDto filter,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        filter.EmployeeId = employeeId;

        return this.ToPagedResponse(
            await _reportService.GetCategoryItemsAsync(
                filter, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateClaimDto dto,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        var result = await _makerService.CreateAsync(
            employeeId, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this,
                result.Error,
                result.ErrorType);
        }

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ClaimResponseDto>.SuccessResponse(
                message: "Claim created",
                data: result.Data!));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateClaimDto dto,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        var result = await _makerService.UpdateAsync(
            employeeId, id, dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this,
                result.Error,
                result.ErrorType);
        }

        return Ok(
            ApiResponse<ClaimResponseDto>.SuccessResponse(
                message: "Claim updated",
                data: result.Data!));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(
        int id,
        CancellationToken cancellationToken)
    {
        if (!TryGetEmployeeId(out var employeeId))
        {
            return InvalidToken();
        }

        var result = await _makerService.CancelAsync(
            employeeId, id, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this,
                result.Error,
                result.ErrorType);
        }

        return Ok(
            ApiResponse<ClaimResponseDto>.SuccessResponse(
                message: "Claim cancelled",
                data: result.Data!));
    }

    // Employee id comes from the token, never from the request body.
    private bool TryGetEmployeeId(out int employeeId)
    {
        var userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("nameid")
            ?? User.FindFirstValue("sub");

        return int.TryParse(userId, out employeeId);
    }

    private IActionResult InvalidToken()
    {
        return Unauthorized(
            ApiErrorResponse.FailureResponse("Invalid token"));
    }
}