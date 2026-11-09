using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Helpers;
using Backend.API.Common.Responses;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.API.Controllers;

[ApiController]
[Route("approver/claims")]
[Authorize(Roles = "Approver")]
public class ApproverController : ControllerBase
{
    private readonly IApproverService _approverService;

    public ApproverController(IApproverService approverService)
    {
        _approverService = approverService;
    }

    // Status defaults to Submitted (the claims waiting for a decision).
    // GET /approver/claims?status=Approved&employeeId=2&from=2026-11-01&to=2026-11-30&index=1&limit=20
    [HttpGet]
    public async Task<IActionResult> GetClaims(
        [FromQuery] ClaimFilterDto filter,
        CancellationToken cancellationToken)
    {
        return this.ToPagedResponse(
            await _approverService.GetClaimsAsync(
                filter, cancellationToken));
    }

    [HttpGet("{id:int}")]
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

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var approverId))
        {
            return InvalidToken();
        }

        var result = await _approverService.ApproveAsync(
            approverId, id, cancellationToken);

        return ToActionResult(result, "Claim approved");
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var approverId))
        {
            return InvalidToken();
        }

        var result = await _approverService.RejectAsync(
            approverId, id, cancellationToken);

        return ToActionResult(result, "Claim rejected");
    }

    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(
        int id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var approverId))
        {
            return InvalidToken();
        }

        var result = await _approverService.ReturnAsync(
            approverId, id, cancellationToken);

        return ToActionResult(result, "Claim returned to maker");
    }

    private IActionResult ToActionResult(
        Result<ApproverClaimDto> result,
        string message)
    {
        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<ApproverClaimDto>.SuccessResponse(
                message: message,
                data: result.Data!));
    }

    private IActionResult InvalidToken()
    {
        return Unauthorized(
            ApiErrorResponse.FailureResponse("Invalid token"));
    }
}