namespace Backend.Domain.Entities;

public class ClaimItemEntity: BaseEntity
{

    public int ClaimId { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public DateOnly ExpenseDate { get; set; }

    public string? Description { get; set; }

    public string? ReceiptUrl { get; set; }
}
