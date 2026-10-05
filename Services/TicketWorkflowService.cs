using SIGRA.Data.Enums;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Services;

public class TicketWorkflowService
{
    public IReadOnlyList<TicketAction> GetAvailableActions(Ticket ticket)
    {
        var transitions = TicketStatusTransitions.GetAllowedTransitions((TicketStatus)ticket.IdStatut);

        return transitions
            // .Where(t => user.IsInRole(t.RequiresRole))
            // .Where(t => t.Condition is null || t.Condition(ticket))
            .Select(t => t.Action)
            .ToList();
    }

    // public Result ExecuteTransition(Ticket ticket, TicketAction action, ClaimsPrincipal user)
    // {
    //     var transitions = TicketWorkflowDefinition.GetTransitionsFrom(ticket.Status);
    //     var rule = transitions.FirstOrDefault(t => t.Action == action);

    //     // REVALIDATION COMPLÈTE — AUCUNE confiance
    //     // accordée à ce que le CLIENT a pu afficher ou envoyer
    //     if (rule is null)
    //         return Result.Failure($"L'action '{action}' n'est pas possible depuis l'état '{ticket.Status}'.");

    //     if (!user.IsInRole(rule.RequiresRole))
    //         return Result.Failure("Vous n'avez pas les droits nécessaires pour cette action.");

    //     if (rule.Condition is not null && !rule.Condition(ticket))
    //         return Result.Failure("Les conditions de cette transition ne sont plus remplies.");

    //     ticket.ChangeStatus(rule.TargetStatus);
    //     return Result.Success();
    // }
}
