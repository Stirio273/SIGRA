using SIGRA.Data.Enums;

namespace SIGRA.Domain;

public static class TicketStatusTransitions
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> ValidTransitions = new()
    {
        [TicketStatus.New] = new[] { TicketStatus.Opened, TicketStatus.PendingReject },
        [TicketStatus.Opened] = new[] { TicketStatus.Pending, TicketStatus.Redirected, TicketStatus.PendingReject, TicketStatus.Solved },
        [TicketStatus.Pending] = new[] { TicketStatus.Redirected },
        [TicketStatus.Redirected] = new[] { TicketStatus.Solved },
        [TicketStatus.PendingReject] = new[] { TicketStatus.New, TicketStatus.Rejected },
        [TicketStatus.Solved] = new[] { TicketStatus.Closed, TicketStatus.Opened },
        [TicketStatus.Closed] = new[] { TicketStatus.Opened }
    };

    public static bool IsValidTransition(TicketStatus from, TicketStatus to) =>
        ValidTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static IReadOnlyList<TicketStatus> GetAllowedTransitions(TicketStatus from) =>
        ValidTransitions.TryGetValue(from, out var allowed)
            ? allowed
            : Array.Empty<TicketStatus>();
}
