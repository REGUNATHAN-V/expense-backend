using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Helpers;
using Backend.API.Common.Responses;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.API.Controllers;

[ApiController]
[Route("lookup")]
[Authorize(Roles = "Approver,Admin")]
public class LookupController : ControllerBase
{
    private readonly IReportService _reportService;

    public LookupController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // Employees for the "filter by employee" picker (approver and admin).
    // GET /lookup/employees
    [HttpGet("employees")]
    public async Task<IActionResult> Employees(
        CancellationToken cancellationToken)
    {
        var result = await _reportService.GetEmployeesAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<List<EmployeeLookupDto>>.SuccessResponse(
                data: result.Data!));
    }
}
