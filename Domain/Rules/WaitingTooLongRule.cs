using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Domain.Rules;

public class WaitingTooLongRule : ITicketAlertRule
{
    private const int ThresholdHours = 48;
    public string AlertType => "WaitingTooLong48h";

    public IQueryable<Ticket> GetCandidates(AppDbContext db)
    {
        var threshold = DateTime.UtcNow.AddHours(-ThresholdHours);

        // Filtre SQL : uniquement les tickets EN ATTENTE
        // depuis plus longtemps que le seuil
        return db.Tickets
            .Where(t => t.IdStatut == (int)TicketStatus.Pending
                     && t.DateChangementStatut <= threshold);
    }

    public Task<bool> ShouldTriggerAsync(Ticket ticket) =>
        Task.FromResult(true);   // Déjà garanti par le filtre SQL ici — pas de calcul additionnel nécessaire

    public string BuildMessage(Ticket ticket) =>
        $"Le ticket #{ticket.NumeroTicket} est en attente client depuis plus de {ThresholdHours}h.";
}
