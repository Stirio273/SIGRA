using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SIGRA.Controllers;
using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Domain;
using SIGRA.Hubs;

namespace SIGRA.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext db,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _hubContext = hubContext;
        _logger = logger;
    }

    // Crée et envoie une notification
    public async Task<Result> SendAsync(
        int userId,
        int idTicket,
        string title,
        string message,
        TypesEvenementNotification eventType,
        Guid? resourceId = null,
        string? resourceType = null)
    {
        // 1) Sauvegarde en base
        var notification = new Notification
        {
            IdDestinataire = userId,
            IdTicket = idTicket,
            IdTypeEvenement = eventType.IdTypeEvenement,
            DateCreation = DateTime.UtcNow,
            // ResourceId = resourceId,
            // ResourceType = resourceType,
            EstLue = false,
            // IsSent = false,
        };

        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();

        // 2) Pousse via SignalR
        var dto = new NotificationDto
        {
            Id = notification.IdNotification,
            // Title = notification.Title,
            Message = eventType.Libelle,
            // EventType = notification.EventType,
            // ResourceId = notification.ResourceId,
            // ResourceType = notification.ResourceType,
            IsRead = false,
            CreatedAt = notification.DateCreation
        };

        try
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", dto);

            // Marque comme envoyée
            // notification.IsSent = true;
            // notification.SentAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // SignalR a échoué mais la notification est en base
            // Elle sera récupérée au prochain chargement
            _logger.LogWarning(ex,
                "Impossible d'envoyer la notification via SignalR " +
                "pour l'utilisateur {userId}.", userId);
        }

        return Result.Success();
    }

    // Récupère toutes les notifications d'un utilisateur
    public async Task<List<NotificationDto>> GetAllAsync(int userId)
    {
        var notifications = await _db.Notifications
            .AsNoTracking()
            .Where(x => x.IdDestinataire == userId)
            .OrderByDescending(x => x.DateCreation)
            .Select(x => new NotificationDto
            {
                Id = x.IdNotification,
                // Title = x.Title,
                Message = x.IdTypeEvenementNavigation.Libelle,
                // EventType = x.EventType,
                // ResourceId = x.ResourceId,
                // ResourceType = x.ResourceType,
                IsRead = x.EstLue,
                CreatedAt = x.DateCreation
            })
            .ToListAsync();

        return notifications;
    }

    // Compte les notifications non lues
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        var count = await _db.Notifications
            .AsNoTracking()
            .CountAsync(x => x.IdDestinataire == userId && !x.EstLue);

        return count;
    }

    // Marque une notification comme lue
    public async Task<Result> MarkAsReadAsync(int notificationId, int userId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(x => x.IdNotification == notificationId && x.IdDestinataire == userId);

        if (notification is null)
            return Result.Failure($"Notification {notificationId} not found.", ErrorType.NotFound);

        notification.EstLue = true;
        notification.DateLecture = DateTime.Now;
        await _db.SaveChangesAsync();

        return Result.Success();
    }

    // Marque toutes les notifications comme lues
    public async Task<Result> MarkAllAsReadAsync(int userId)
    {
        await _db.Notifications
            .Where(x => x.IdDestinataire == userId && !x.EstLue)
            .ExecuteUpdateAsync(x => x
                .SetProperty(n => n.EstLue, true)
                .SetProperty(n => n.DateLecture, DateTime.Now));

        return Result.Success();
    }
}
