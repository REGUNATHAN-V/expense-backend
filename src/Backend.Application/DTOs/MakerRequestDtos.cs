using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs;

public class ClaimItemDto
{
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0.01, 10000000)]
    public decimal Amount { get; set; }

    public DateOnly ExpenseDate { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    [MaxLength(300)]
    public string? ReceiptUrl { get; set; }
}

public class SaveClaimDto
{
    public DateOnly ClaimDate { get; set; }
    public string? Signature { get; set; }

    [Required]
    [MinLength(1)]
    public List<ClaimItemDto> Items { get; set; } = new();

    public bool Submit { get; set; }
}

public class CreateClaimDto : SaveClaimDto
{
}

public class UpdateClaimDto : SaveClaimDto
{
}
