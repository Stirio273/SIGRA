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

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Récupère toutes les notifications au chargement
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        var notifications = await _notificationService.GetAllAsync(userId);

        return Ok(notifications);
    }

    // Nombre de notifications non lues
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        var result = await _notificationService.GetUnreadCountAsync(userId);

        return Ok(result);
    }

    // Marque une notification comme lue
    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetUserId();
        var result = await _notificationService.MarkAsReadAsync(id, userId);

        if (!result.IsSuccess)
            return result.ToHttpResult();

        return NoContent();
    }

    // Marque toutes les notifications comme lues
    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var userId = GetUserId();
        var result = await _notificationService.MarkAllAsReadAsync(userId);

        if (!result.IsSuccess)
            return result.ToHttpResult();

        return NoContent();
    }

    private int GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Int16.Parse(claim!);
    }
}
