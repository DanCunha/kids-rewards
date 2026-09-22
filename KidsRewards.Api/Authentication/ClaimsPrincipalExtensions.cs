using System.Security.Claims;

namespace KidsRewards.Api.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    public static UserContext ToUserContext(this ClaimsPrincipal principal) =>
        new(
            principal.GetUserId(),
            principal.FindFirstValue(AuthConstants.TypeClaim) ?? string.Empty,
            principal.FindFirstValue(AuthConstants.ChildIdClaim));
}
