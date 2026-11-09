using System.Security.Claims;

namespace Backend.API.Common.Helpers;

public static class ClaimsPrincipalExtensions
{
    // The user id always comes from the token, never from the request body.
    public static bool TryGetUserId(this ClaimsPrincipal user, out int userId)
    {
        var value =
            user.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? user.FindFirstValue("nameid")
            ?? user.FindFirstValue("sub");

        return int.TryParse(value, out userId);
    }
}