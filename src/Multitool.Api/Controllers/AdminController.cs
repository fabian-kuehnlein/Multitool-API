using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.User;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Returns all users for the administration area.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userService.GetUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Creates a new user with a temporary password.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("users")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var id = await userService.CreateUserAsync(dto);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    /// <summary>
    /// Sets a new temporary password for the given user and clears an existing lockout.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}/password")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromRoute] int id, [FromBody] ResetPasswordDto dto)
    {
        await userService.ResetPasswordAsync(id, dto);
        return NoContent();
    }

    /// <summary>
    /// Activates or deactivates the given user.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("users/{id}/active")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetUserActive([FromRoute] int id, [FromBody] SetUserActiveDto dto)
    {
        await userService.SetUserActiveAsync(id, dto);
        return NoContent();
    }
}
