using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Extensions;
using Multitool.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    /// <summary>
    /// Logs in a user with the provided username and password and returns a JWT as session token if credentials are valid
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [Produces("application/json")]
    [EnableRateLimiting("login-limit")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await authenticationService.LoginAsync(request.Username, request.Password);
        return Ok(new { token });
    }
}
