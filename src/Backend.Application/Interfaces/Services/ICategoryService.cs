using Backend.Application.Common.Results;
using Backend.Application.DTOs;

namespace Backend.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<Result<List<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Result<List<LeaveTypeDto>>> GetAllLeaveTypes(
        CancellationToken cancellationToken = default);
}