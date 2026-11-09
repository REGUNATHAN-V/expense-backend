using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Responses;
using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.API.Common.Helpers;

public static class PagedResponseHelper
{
    // Turns a paged service result into your ApiResponse with Pagination filled in.
    public static IActionResult ToPagedResponse<T>(
        this ControllerBase controller,
        Result<PagedResult<T>> result)
    {
        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                controller, result.Error, result.ErrorType);
        }

        var page = result.Data!;

        return controller.Ok(
            ApiResponse<List<T>>.SuccessResponse(
                data: page.Items,
                pagination: new PaginationDto
                {
                    HasNext = page.HasNext,
                    HasPrevious = page.HasPrevious,
                    Index = page.Index,
                    Limit = page.Limit
                }));
    }
}