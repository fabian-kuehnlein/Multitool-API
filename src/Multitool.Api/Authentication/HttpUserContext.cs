using Multitool.Api.Extensions;
using Multitool.Domain.Interfaces;

namespace Multitool.Api.Authentication;

/// <summary>
/// Reads the current user id from the JWT of the active HTTP request.
/// Outside of a request (e.g. background jobs) or for anonymous requests (e.g. login)
/// it returns 0, so no user data is exposed.
/// </summary>
public sealed class HttpUserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public int UserId => httpContextAccessor.HttpContext?.User.GetUserIdOrDefault() ?? 0;
}
