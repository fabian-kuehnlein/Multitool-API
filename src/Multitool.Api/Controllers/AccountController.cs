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
    /// Updates the username and password of the authenticated user and returns a new JWT.
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
    public async Task<IActionResult> UpdateCredentials([FromBody] UpdateAccountCredentialsDto dto)
    {
        var token = await accountService.UpdateAccountCredentialsAsync(dto);
        return Ok(new { token });
    }
}
