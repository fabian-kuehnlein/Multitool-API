using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController() : ControllerBase
{
    [HttpGet("live")]
    [AllowAnonymous]
    public IActionResult Live() =>
        Ok(new { status = "alive", timestamp = DateTime.UtcNow });
}