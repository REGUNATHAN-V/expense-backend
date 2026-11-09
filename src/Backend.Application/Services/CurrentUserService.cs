using Backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
namespace Backend.Application.Services;



public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public int? UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;

            var userId =
                user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user?.FindFirst("sub")?.Value;

            return int.TryParse(userId, out var id) ? id : null;
        }
    }
}