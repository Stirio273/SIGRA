using SIGRA.Data;
using Microsoft.EntityFrameworkCore;
using SIGRA.Domain;

namespace SIGRA.Services.Handlers;

public class ResolveAllAlertsOnCloseHandler : IDomainEventHandler<TicketClosedEvent>
{
    private readonly AppDbContext _db;

    public async Task HandleAsync(TicketClosedEvent domainEvent)
    {
        var activeAlerts = await _db.AlerteTickets
            .Where(a => a.IdTicket == domainEvent.TicketId && a.DateExpiration == null)
            .ToListAsync();

        foreach (var alert in activeAlerts)
            alert.Resolve();

        await _db.SaveChangesAsync();
    }
}