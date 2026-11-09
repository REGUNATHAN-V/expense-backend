namespace Backend.Domain.Entities;

public class UserEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;
    public int Role { get; set; } = 1; // 1 - Maker, 2 - Approver, 3- admin
    public bool IsActive { get; set; } = true;
}
