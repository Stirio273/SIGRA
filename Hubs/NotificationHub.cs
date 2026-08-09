using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SIGRA.Services;

namespace SIGRA.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly IUserAuthenticationService _userAuthenticationService;

    public NotificationHub(ILogger<NotificationHub> logger, IUserAuthenticationService userAuthenticationService)
    {
        _logger = logger;
        _userAuthenticationService = userAuthenticationService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = await GetUserIdAsync();

        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId.Value}");
            _logger.LogInformation("Utilisateur {userId} connecté.", userId.Value);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = await GetUserIdAsync();

        if (userId.HasValue)
            _logger.LogInformation("Utilisateur {userId} déconnecté.", userId.Value);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task<int?> GetUserIdAsync()
    {
        var username = Context.User?.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return null;

        var user = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        return user?.IdUtilisateur;
    }
}