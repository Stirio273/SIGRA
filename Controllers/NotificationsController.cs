using Microsoft.AspNetCore.Mvc;
using SIGRA.Data.Models;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly DeltaQueryService _deltaQueryService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        DeltaQueryService deltaQueryService,
        ILogger<NotificationsController> logger)
    {
        _deltaQueryService = deltaQueryService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ReceiveNotification(
        [FromQuery] string validationToken = null)
    {
        // Step 1: Handle webhook validation challenge
        if (!string.IsNullOrEmpty(validationToken))
        {
            _logger.LogInformation("Webhook validation received.");
            return Content(validationToken, "text/plain");
        }

        // Step 2: Read notification payload
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync();
        _logger.LogInformation("Notification received: {Body}", body);

        // Step 3: Validate client state (security check)
        var notifications = System.Text.Json.JsonSerializer
            .Deserialize<NotificationCollection>(body);

        foreach (var notification in notifications.Value)
        {
            if (notification.ClientState != "SecretClientState123")
            {
                _logger.LogWarning("Invalid client state received!");
                continue;
            }

            // Step 4: Trigger delta sync on notification
            _logger.LogInformation("Valid notification - triggering delta sync...");
            await _deltaQueryService.SyncMessagesAsync();
        }

        return Accepted();
    }
}