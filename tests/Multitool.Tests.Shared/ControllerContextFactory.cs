using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Multitool.Tests.Shared;

/// <summary>
/// Creates a <see cref="ControllerContext"/> with an authenticated user, so controllers
/// can read the user id claim (<c>ClaimTypes.NameIdentifier</c>) like from a real JWT.
/// </summary>
public static class ControllerContextFactory
{
    public static ControllerContext Create(int userId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        ],
        authenticationType: "Test");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
