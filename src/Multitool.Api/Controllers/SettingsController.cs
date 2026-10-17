using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Extensions;
using Multitool.Application.Interfaces;
using Multitool.Application.Models.Settings;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController(ISettingsService service) : ControllerBase
{
    /// <summary>
    /// Returns the complete settings object of the current user.
    /// Defined keys that are not stored yet are created lazily with their default value.
    /// </summary>
    [Authorize]
    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await service.GetSettingsAsync(User.GetUserId());
        return Ok(settings);
    }

    /// <summary>
    /// Validates and stores a single setting value for the given module and key.
    /// </summary>
    [Authorize]
    [HttpPatch("{module}/{key}")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateSetting([FromRoute] string module, [FromRoute] string key, [FromBody] UpdateSettingDto dto)
    {
        await service.UpdateSettingAsync(User.GetUserId(), module, key, dto);
        return NoContent();
    }
}
