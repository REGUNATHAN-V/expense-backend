
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Responses;
using Backend.Application.Common.Results;

namespace Backend.API.Common.Helpers;

public static class ControllerResponseHelper
{
    public static IActionResult HandleFailure(
        ControllerBase controller,
        string? error,
        ResultErrorType errorType)
    {
        var response = ApiErrorResponse.FailureResponse(
            error ?? "An unexpected error occurred");

        return errorType switch
        {
            ResultErrorType.Validation =>
                controller.BadRequest(response),

            ResultErrorType.Conflict =>
                controller.Conflict(response),

            ResultErrorType.Unauthorized =>
                controller.Unauthorized(response),

            ResultErrorType.Forbidden =>
                controller.StatusCode(
                    StatusCodes.Status403Forbidden,
                    response),

            ResultErrorType.NotFound =>
                controller.NotFound(response),

            _ =>
                controller.StatusCode(
                    StatusCodes.Status500InternalServerError,
                    response)
        };
    }
}
