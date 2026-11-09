namespace Backend.Application.Interfaces.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
}