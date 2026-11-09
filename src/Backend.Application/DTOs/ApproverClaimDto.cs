using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs;

// A claim as the approver sees it: the normal claim plus who owns it.
public class ApproverClaimDto : ClaimResponseDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? DecidedAt { get; set; }
}

public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UpdateRoleDto
{
    // 1 = Maker, 2 = Approver, 3 = Admin
    [Range(1, 3)]
    public int Role { get; set; }
}

public class SetActiveDto
{
    public bool IsActive { get; set; }
}