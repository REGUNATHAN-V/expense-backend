using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;
using Backend.Domain.Entities;
using Backend.Domain.Enums;

namespace Backend.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAuthRepository _userRepository;

    public AdminService(IAuthRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PagedResult<UserResponseDto>>> GetUsersAsync(
        PagedRequestDto paging,
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.FindAsync(
            user => !user.IsDeleted, cancellationToken);

        var all = users
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Id)
            .Select(ToResponse);

        return Result<PagedResult<UserResponseDto>>.Success(
            PagedResult<UserResponseDto>.FromList(
                all, paging.Index, paging.Limit));
    }

    public async Task<Result<UserResponseDto>> UpdateRoleAsync(
        int adminId,
        int userId,
        UpdateRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(UserRole), dto.Role))
        {
            return Result<UserResponseDto>.Failure(
                "Role must be 1 (Maker), 2 (Approver) or 3 (Admin)",
                ResultErrorType.Validation);
        }

        // Stops the last admin from locking everyone out.
        if (userId == adminId)
        {
            return Result<UserResponseDto>.Failure(
                "You cannot change your own role",
                ResultErrorType.Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(
            userId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            return Result<UserResponseDto>.Failure(
                "User not found",
                ResultErrorType.NotFound);
        }

        user.Role = dto.Role;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result<UserResponseDto>.Success(ToResponse(user));
    }

    public async Task<Result<UserResponseDto>> SetActiveAsync(
        int adminId,
        int userId,
        SetActiveDto dto,
        CancellationToken cancellationToken = default)
    {
        if (userId == adminId)
        {
            return Result<UserResponseDto>.Failure(
                "You cannot change your own active status",
                ResultErrorType.Forbidden);
        }

        var user = await _userRepository.GetByIdAsync(
            userId, cancellationToken);

        if (user is null || user.IsDeleted)
        {
            return Result<UserResponseDto>.Failure(
                "User not found",
                ResultErrorType.NotFound);
        }

        user.IsActive = dto.IsActive;

        await _userRepository.SaveChangesAsync(cancellationToken);

        return Result<UserResponseDto>.Success(ToResponse(user));
    }

    private static UserResponseDto ToResponse(UserEntity user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = Enum.IsDefined(typeof(UserRole), user.Role)
                ? ((UserRole)user.Role).ToString()
                : user.Role.ToString(),
            IsActive = user.IsActive
        };
    }
}