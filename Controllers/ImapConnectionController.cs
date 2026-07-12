using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/imap")]
public class ImapConnectionController : ControllerBase
{
    private readonly ImapMailService _imapMailService;
    private readonly ILogger<ImapConnectionController> _logger;

    public ImapConnectionController(ImapMailService imapMailService, ILogger<ImapConnectionController> logger)
    {
        _imapMailService = imapMailService;
        _logger = logger;
    }

    [HttpPost("test")]
    public async Task<IActionResult> TestConnection()
    {
        try
        {
            using var client = await _imapMailService.ConnectAsync();
            await client.DisconnectAsync(true);
            return Ok(new { Message = "IMAP connection successful." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "IMAP connection failed");
            return StatusCode(500, new { Message = "IMAP connection failed.", Detail = ex.Message });
        }
    }
}
