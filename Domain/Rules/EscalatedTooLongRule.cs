using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Domain.Rules;

public class EscalatedTooLongRule : ITicketAlertRule
{
    private const int ThresholdHours = 48;
    public string AlertType => "EscalatedTooLong48h";

    public IQueryable<Ticket> GetCandidates(AppDbContext db)
    {
        var threshold = DateTime.UtcNow.AddHours(-ThresholdHours);

        return db.Tickets
            .Where(t => t.IdStatut == (int)TicketStatus.Redirected
                     && t.IdStatut != (int)TicketStatus.Closed
                     && t.Escalades.OrderBy(e => e.DateEscalade).Last().DateEscalade <= threshold);
    }

    public Task<bool> ShouldTriggerAsync(Ticket ticket) => Task.FromResult(true);

    public string BuildMessage(Ticket ticket) =>
        $"Le ticket #{ticket.NumeroTicket} est escaladé depuis plus de {ThresholdHours}h sans résolution.";
}
