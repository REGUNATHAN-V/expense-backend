using Backend.Application.Common.Results;
using Backend.Application.DTOs;
using Backend.Application.Interfaces.Repositories;
using Backend.Application.Interfaces.Services;

namespace Backend.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IClaimRepository _claimRepository;

    public CategoryService(IClaimRepository claimRepository)
    {
        _claimRepository = claimRepository;
    }

    public async Task<Result<List<CategoryDto>>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await _claimRepository.GetAllCategoriesAsync(
            cancellationToken);

        return Result<List<CategoryDto>>.Success(
            categories
                .Select(category => new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    MaxPerItem = category.MaxPerItem
                })
                .ToList());
    }

    public async Task<Result<List<LeaveTypeDto>>> GetAllLeaveTypes(
        CancellationToken cancellationToken = default)
    {
        var leave = await _claimRepository.GetAllLeaveTypesAsync(
            cancellationToken);

        return Result<List<LeaveTypeDto>>.Success(
            leave
                .Select(leave => new LeaveTypeDto
                {
                    Id = leave.Id,
                    Code= leave.Code,
                    Name = leave.Name,
                    MaxDaysPerYear = leave.MaxDaysPerYear,
                    RequiresAttachment = leave.RequiresAttachment

                })
                .ToList());
    }
}