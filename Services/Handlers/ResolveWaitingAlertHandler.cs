using SIGRA.Data;
using Microsoft.EntityFrameworkCore;
using SIGRA.Domain;

namespace SIGRA.Services.Handlers;

public class ResolveWaitingAlertHandler : IDomainEventHandler<TicketResumedEvent>
{
    private readonly AppDbContext _db;

    public async Task HandleAsync(TicketResumedEvent domainEvent)
    {
        await ResolveAlertAsync(domainEvent.TicketId, "WaitingTooLong48h");
    }

    private async Task ResolveAlertAsync(int ticketId, string alertType)
    {
        var alert = await _db.AlerteTickets.FirstOrDefaultAsync(a =>
            a.IdTicket == ticketId && a.TypeAlerte == alertType && a.DateExpiration == null);

        alert?.Expirer();
        await _db.SaveChangesAsync();
    }
}