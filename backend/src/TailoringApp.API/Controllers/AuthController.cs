using Microsoft.AspNetCore.Mvc;
using TailoringApp.Application.Abstractions.Auth;

namespace TailoringApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    public AuthController(IAuthService auth) => _auth = auth;

    public sealed record LoginRequest(string Username, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _auth.ValidateUserAsync(req.Username, req.Password, ct);
        if (user is null) return Unauthorized();
        var token = _auth.GenerateToken(user);
        return Ok(new { token, role = user.Role, username = user.Username });
    }
}
