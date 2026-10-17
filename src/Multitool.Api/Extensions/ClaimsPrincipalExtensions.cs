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
}
