using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Account;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController(IAccountService accountService) : ControllerBase
{
    /// <summary>
    /// Confirms the current username and password of the authenticated user so
    /// they may proceed to change their account data.
    /// </summary>
    [Authorize]
    [HttpPost("credentials/verify")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyCredentials([FromBody] VerifyAccountCredentialsRequest request)
    {
        await accountService.VerifyAccountCredentialsAsync(request);
        return NoContent();
    }

    /// <summary>
    /// Sets the username and password of the authenticated user after logging in
    /// with a temporary password and returns a new JWT.
    /// </summary>
    [Authorize]
    [HttpPut("credentials/setup")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetupCredentials([FromBody] SetupAccountCredentialsRequest request)
    {
        var token = await accountService.SetupAccountCredentialsAsync(request);
        return Ok(new { token });
    }

    /// <summary>
    /// Changes the username and/or password of the authenticated user after
    /// confirming the current password and returns a new JWT.
    /// </summary>
    [Authorize]
    [HttpPut("credentials")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeCredentials([FromBody] ChangeAccountCredentialsRequest request)
    {
        var token = await accountService.ChangeAccountCredentialsAsync(request);
        return Ok(new { token });
    }
}
