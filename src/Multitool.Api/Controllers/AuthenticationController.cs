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
    /// Registers a new user with the provided username and password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register(RegisterRequest request, [FromHeader(Name = "X-Admin-Key")] string adminKey)
    {
        await authenticationService.RegisterAsync(request.Username, request.Password, adminKey);
        return Ok("User created");
    }

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