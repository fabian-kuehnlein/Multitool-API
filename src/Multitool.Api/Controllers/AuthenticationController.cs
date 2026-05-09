using Microsoft.AspNetCore.Mvc;
using Multitool.Api.Extensions;
using Multitool.Application.Interfaces;

namespace Multitool.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await authenticationService.RegisterAsync(request.Username, request.Password);
        return Ok("User created");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var token = await authenticationService.LoginAsync(request.Username, request.Password);
        return Ok(new { token });
    }
}