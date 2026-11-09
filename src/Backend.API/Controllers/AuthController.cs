
using Microsoft.AspNetCore.Mvc;
using Backend.API.Common.Helpers;
using Backend.API.Common.Responses;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Services;

namespace Backend.API.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(
            dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this,
                result.Error,
                result.ErrorType);
        }

        return Ok(
            ApiResponse<AuthResponseDto>.SuccessResponse(
                data: result.Data!));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            dto, cancellationToken);

        if (!result.IsSuccess)
        {
            return ControllerResponseHelper.HandleFailure(
                this,
                result.Error,
                result.ErrorType);
        }

        return Ok(
            ApiResponse<AuthResponseDto>.SuccessResponse(data:result.Data!));
    }

    // [HttpPost("forgot-password")]
    // public async Task<IActionResult> ForgotPassword(
    //     [FromBody] ForgotPasswordDto dto, 
    //     CancellationToken cancellationToken)
    // {
    //     var result  = await _authService.ForgotPasswordAsync(
    //         dto, cancellationToken);

    //     if (!result.IsSuccess)
    //     {
    //         return ControllerResponseHelper.HandleFailure(
    //             this,
    //             result.Error,
    //             result.ErrorType);
    //     }

    //     return Ok();

    // }

}
