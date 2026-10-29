using System.Security.Claims;

namespace Multitool.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the id of the authenticated user from the JWT name identifier claim.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("The token does not contain a user id claim."));

    /// <summary>
    /// Returns the id of the authenticated user, or 0 if the request is anonymous
    /// or the claim is missing/invalid. Used where no user is expected (e.g. login, background jobs).
    /// </summary>
    public static int GetUserIdOrDefault(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : 0;
}
