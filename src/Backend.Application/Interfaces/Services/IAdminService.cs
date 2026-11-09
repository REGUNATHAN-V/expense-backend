using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface IAdminService
{
    Task<Result<PagedResult<UserResponseDto>>> GetUsersAsync(
        PagedRequestDto paging,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponseDto>> UpdateRoleAsync(
        int adminId,
        int userId,
        UpdateRoleDto dto,
        CancellationToken cancellationToken = default);

    Task<Result<UserResponseDto>> SetActiveAsync(
        int adminId,
        int userId,
        SetActiveDto dto,
        CancellationToken cancellationToken = default);
}