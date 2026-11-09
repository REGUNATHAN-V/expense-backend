using Backend.Domain.Enums;

namespace Backend.Domain.Entities;

public class ClaimEntity : BaseEntity
{

    public int EmployeeId { get; set; }

    public DateOnly ClaimDate { get; set; }
    public string? Signature { get; set; }

    public decimal TotalAmount { get; set; }

    public ClaimStatus Status { get; set; } = ClaimStatus.Draft;

    public int? ApproverId { get; set; }

    public DateTime? DecidedAt { get; set; }

    public List<ClaimItemEntity> Items { get; set; } = new();
}
