using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace SIGRA.Hubs;

public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        // Chaque utilisateur rejoint son groupe personnel
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            _logger.LogInformation("Utilisateur {userId} connecté.", userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is not null)
            _logger.LogInformation("Utilisateur {userId} déconnecté.", userId);

        await base.OnDisconnectedAsync(exception);
    }
}
