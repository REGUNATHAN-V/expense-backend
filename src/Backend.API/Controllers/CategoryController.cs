using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Helpers;
using Backend.API.Common.Responses;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.API.Controllers;

[ApiController]
[Route("categories")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Any logged-in user. Makers need the ids to build a claim.
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<List<CategoryDto>>.SuccessResponse(
                data: result.Data!));
    }


[Route("leavetype")]
[Authorize]
     [HttpGet]
    public async Task<IActionResult> GetAllLeaveTypes(
        CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllLeaveTypes(cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this, result.Error, result.ErrorType);
        }

        return Ok(
            ApiResponse<List<LeaveTypeDto>>.SuccessResponse(
                data: result.Data!));
    }
}