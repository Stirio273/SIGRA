namespace SIGRA.Domain.AIsupport;

public sealed class ResolvedTicketSummary
{
    public required int IdTicket { get; init; }

    public string Application { get; init; }

    public required string Title { get; init; }

    public required string ResolutionNotes { get; init; }

    public string? Module { get; init; }

    // public ResolutionType ResolutionType { get; init; }

    public string? ProblemRecord { get; init; }

    public DateTimeOffset ResolvedAt { get; init; }
}
