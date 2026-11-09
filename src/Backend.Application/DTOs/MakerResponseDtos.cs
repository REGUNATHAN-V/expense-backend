namespace Backend.Application.DTOs;

public class ClaimItemResponseDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
    public string? ReceiptUrl { get; set; }
}

public class ClaimResponseDto
{
    public int Id { get; set; }
    public DateOnly ClaimDate { get; set; }
    public string? Signature { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ClaimItemResponseDto> Items { get; set; } = new();
}
