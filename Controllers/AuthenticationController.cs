using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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

        if (user == null)
        {
            return NotFound(new { Message = "User not found." });
        }

        return Ok(new
        {
            UserGuid = user.UserGuid,
            User = User.Identity?.Name,
            Type = User.Identity?.AuthenticationType,
            Role = user.IdRoleNavigation?.Libelle
        });
    }

    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new { Message = "This endpoint is public." });
    }
}
