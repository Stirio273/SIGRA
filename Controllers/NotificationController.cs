using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGRA.Services;

namespace SIGRA.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IUserAuthenticationService _userAuthenticationService;

    public NotificationController(INotificationService notificationService, IUserAuthenticationService userAuthenticationService)
    {
        _notificationService = notificationService;
        _userAuthenticationService = userAuthenticationService;
    }

    // Récupère toutes les notifications au chargement
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = await GetUserIdAsync();
        var notifications = await _notificationService.GetAllAsync(userId);

        return Ok(notifications);
    }

    // Nombre de notifications non lues
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = await GetUserIdAsync();
        var result = await _notificationService.GetUnreadCountAsync(userId);

        return Ok(result);
    }

    // Marque une notification comme lue
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = await GetUserIdAsync();
        var result = await _notificationService.MarkAsReadAsync(id, userId);

        if (!result.IsSuccess)
            return result.ToHttpResult();

        return NoContent();
    }

    // Marque toutes les notifications comme lues
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = await GetUserIdAsync();
        var result = await _notificationService.MarkAllAsReadAsync(userId);

        if (!result.IsSuccess)
            return result.ToHttpResult();

        return NoContent();
    }

    private async Task<int> GetUserIdAsync()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            throw new UnauthorizedAccessException("No authenticated user found.");

        var user = await _userAuthenticationService.GetAuthorizedUserAsync(username);
        if (user == null)
            throw new UnauthorizedAccessException($"User '{username}' is not authorized.");

        return user.IdUtilisateur;
    }
}
