using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly MailAuthService _mailAuthService;
    private readonly WebhookService _webhookService;

    public AuthController(MailAuthService mailAuthService, WebhookService webhookService)
    {
        _mailAuthService = mailAuthService;
        _webhookService = webhookService;
    }

    // Redirect user to Microsoft login
    [HttpGet("login")]
    public async Task<IActionResult> Login()
    {
        var url = await _mailAuthService.GetAuthorizationUrlAsync();
        return Redirect(url);
    }

    // Handle OAuth callback
    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code)
    {
        await _mailAuthService.ExchangeCodeForTokenAsync(code);

        // Create webhook subscription after login
        await _webhookService.CreateOrRenewSubscriptionAsync();

        return Ok("Authentication successful! Webhook subscription created.");
    }
}