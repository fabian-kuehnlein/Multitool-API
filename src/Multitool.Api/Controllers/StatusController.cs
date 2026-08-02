using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController() : ControllerBase
{
    /// <summary>
    /// Returns the liveness status of the API.
    /// </summary>
    [HttpGet("live")]
    [AllowAnonymous]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Live() =>
        Ok(new { status = "alive", timestamp = DateTime.UtcNow });
}