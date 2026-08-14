using SIGRA.Data;
using SIGRA.Data.Models;

namespace SIGRA.Domain.Rules;

public interface ITicketAlertRule
{
    string AlertType { get; }

    // Filtre SQL LÉGER — réduit drastiquement le nombre de tickets
    // à charger en mémoire pour l'évaluation précise
    IQueryable<Ticket> GetCandidates(AppDbContext db);

    // Évaluation PRÉCISE sur les candidats déjà filtrés
    Task<bool> ShouldTriggerAsync(Ticket ticket);

    string BuildMessage(Ticket ticket);
}
