using SIGRA.Data.Enums;
using SIGRA.Data.Models;

namespace SIGRA.Domain;

public static class TicketActionPolicy
{
    public static bool IsActionAvailable(TicketAction action, Ticket ticket, string? userRole, int? userId)
    {
        if (string.IsNullOrEmpty(userRole))
            return false;

        if (userRole == "Administrateur")
            return true;

        if (userRole != "Technicien")
            return false;

        if (ticket.IdTechnicienAssigne == null)
            return action == TicketAction.Assign;

        if (ticket.IdTechnicienAssigne != userId)
            return false;

        return true;
    }

    public static IReadOnlyList<TicketAction> GetAvailableActions(Ticket ticket, string? userRole, int? userId)
    {
        var from = (TicketStatus)ticket.IdStatut;
        var statusActions = TicketStatusTransitions.GetAllowedTransitions(from)
            .Select(r => r.Action);

        var globalActions = new[] { TicketAction.Reassign };

        var candidates = statusActions.Concat(globalActions).Distinct().ToList();

        return candidates
            .Where(a => IsActionAvailable(a, ticket, userRole, userId))
            .ToList();
    }
}
