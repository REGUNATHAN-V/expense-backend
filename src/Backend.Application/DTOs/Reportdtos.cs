using Backend.Domain.Enums;

namespace Backend.Application.DTOs;

// Optional filters shared by the report endpoints.
// From/To apply to the expense date (monthly totals) or the claim date
// (mismatched and latest claims). CategoryId limits monthly totals and items
// to that category, and limits claim reports to claims that contain it.
public class ReportFilterDto : PagedRequestDto
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public int? EmployeeId { get; set; }
    public int? CategoryId { get; set; }
    public ClaimStatus? Status { get; set; }
}

public class MonthlyTotalDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;

    // "2026-11". Null for an employee with no claims.
    public string? Month { get; set; }

    public int? CategoryId { get; set; }
    public string? Category { get; set; }
    public decimal Total { get; set; }
}

public class TopCategoryDto
{
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class MismatchedClaimDto
{
    public int ClaimId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public DateOnly ClaimDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal ItemsSum { get; set; }
    public decimal Difference { get; set; }
}

public class LatestClaimDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ClaimId { get; set; }
    public DateOnly ClaimDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? DecidedAt { get; set; }
}

// One expense item with its claim, maker and approver. This is what sits
// behind a monthly total (employee + month + category).
public class ExpenseItemDto
{
    public int ItemId { get; set; }
    public int ClaimId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
}

// Employee picker for the approver and admin filters.
public class EmployeeLookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}