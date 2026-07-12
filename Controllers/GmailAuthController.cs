using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/imap/oauth2")]
public class GmailAuthController : ControllerBase
{
    private readonly IImapIdentityProvider _identityProvider;

    public GmailAuthController(IImapIdentityProvider identityProvider)
    {
        _identityProvider = identityProvider;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromQuery] string? redirect_uri)
    {
        // Le frontend peut passer un redirect_uri dynamique si besoin
        var url = await _identityProvider.GetAuthorizationUrlAsync(redirect_uri);
        if (string.IsNullOrEmpty(url))
            return BadRequest("Unable to build authorization URL.");

        return Redirect(url);
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string? redirect_uri)
    {
        if (string.IsNullOrEmpty(code))
            return BadRequest("Authorization code is required.");

        await _identityProvider.ExchangeCodeAsync(code);

        return Ok("Gmail OAuth2 authentication successful. You can close this page.");
    }
}