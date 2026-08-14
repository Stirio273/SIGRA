using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Services.Handlers;

public interface IDomainEventHandler<TEvent>
{
    Task HandleAsync(TEvent domainEvent);
}

// Chaque effet de bord devient sa PROPRE classe, testable isolément
public class RecordReopenHistoryHandler : IDomainEventHandler<TicketReopenedEvent>
{
    private readonly AppDbContext _db;

    public async Task HandleAsync(TicketReopenedEvent domainEvent)
    {
        _db.HistoriqueStatuts.Add(new HistoriqueStatut
        {
            Id = Guid.NewGuid(),
            TicketId = domainEvent.TicketId,
            OriginalClosedAt = domainEvent.OriginalClosedAt,
            ReopenedAt = DateTime.UtcNow,
            ReopenedByUserId = domainEvent.ReopenedByUserId,
            Reason = domainEvent.Reason
        });

        await _db.SaveChangesAsync();
    }
}

public class NotifyTicketReopenedHandler : IDomainEventHandler<TicketReopenedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly AppDbContext _db;

    public async Task HandleAsync(TicketReopenedEvent domainEvent)
    {
        var ticket = await _db.Tickets.FindAsync(domainEvent.TicketId);
        await _notificationService.NotifyTicketReopenedAsync(ticket!, domainEvent.ReopenedByUserId);
    }
}

public class AlertOnRepeatedReopenHandler : IDomainEventHandler<TicketReopenedEvent>
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public async Task HandleAsync(TicketReopenedEvent domainEvent)
    {
        if (domainEvent.ReopenCount >= 3)
        {
            await _hubContext.Clients.Group("team-managers").SendAsync("ReceiveAlert", new
            {
                Type = "RepeatedReopen",
                TicketId = domainEvent.TicketId
            });
        }
    }
}
