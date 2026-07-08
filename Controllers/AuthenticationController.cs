using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ADAuthorizedUser")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserAuthenticationService _authService;

    public AuthenticationController(IUserAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var username = User.Identity?.Name ?? "Unknown";
        var user = await _authService.GetAuthorizedUserAsync(username);

        return Ok(new
        {
            AdUsername = username,
            Email = user?.Email,
            Role = user?.Role,
            Message = "Access granted"
        });
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { Message = "This endpoint is public." });
    }
}
