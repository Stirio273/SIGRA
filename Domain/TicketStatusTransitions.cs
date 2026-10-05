using SIGRA.Data.Enums;

namespace SIGRA.Domain;

public record TransitionRule(
    TicketAction Action,
    TicketStatus TargetStatus
);

public static class TicketStatusTransitions
{
    private static readonly Dictionary<TicketStatus, TransitionRule[]> StatusTransitions = new()
    {
        [TicketStatus.New] = new[] { new TransitionRule(TicketAction.Assign, TicketStatus.Opened), new TransitionRule(TicketAction.AskForReject, TicketStatus.PendingReject) },
        [TicketStatus.Opened] = new[] { new TransitionRule(TicketAction.ChangeStatus, TicketStatus.Pending), new TransitionRule(TicketAction.Transfer, TicketStatus.Redirected),
         new TransitionRule(TicketAction.AskForReject, TicketStatus.PendingReject), new TransitionRule(TicketAction.ChangeStatus, TicketStatus.Solved) },
        [TicketStatus.Pending] = new[] { new TransitionRule(TicketAction.Transfer, TicketStatus.Redirected) },
        [TicketStatus.Redirected] = new[] { new TransitionRule(TicketAction.ChangeStatus, TicketStatus.Solved) },
        [TicketStatus.PendingReject] = new[] { new TransitionRule(TicketAction.ChangeStatus, TicketStatus.New), new TransitionRule(TicketAction.Reject, TicketStatus.Rejected) },
        [TicketStatus.Solved] = new[] { new TransitionRule(TicketAction.Close, TicketStatus.Closed), new TransitionRule(TicketAction.ChangeStatus, TicketStatus.Opened) },
        [TicketStatus.Closed] = new[] { new TransitionRule(TicketAction.ChangeStatus, TicketStatus.Opened) }
    };

    public static bool IsValidTransition(TicketStatus from, TicketStatus to) =>
        StatusTransitions.TryGetValue(from, out var allowed) && allowed.Select(rule => rule.TargetStatus).Contains(to);

    public static IReadOnlyList<TransitionRule> GetAllowedTransitions(TicketStatus from) =>
        StatusTransitions.TryGetValue(from, out var allowed)
            ? allowed
            : Array.Empty<TransitionRule>();
}
