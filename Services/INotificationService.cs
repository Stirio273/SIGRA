using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Services;

public interface INotificationService
{
    Task<Result> SendAsync(
        int userId,
        int idTicket,
        string title,
        string message,
        TypesEvenementNotification eventType,
        Guid? resourceId = null,
        string? resourceType = null);

    Task<List<NotificationDto>> GetAllAsync(int userId);
    Task<int> GetUnreadCountAsync(int userId);
    Task<Result> MarkAsReadAsync(int notificationId, int userId);
    Task<Result> MarkAllAsReadAsync(int userId);
    Task NotifyAlertAsync(Ticket ticket, string message, string alertType);
}
