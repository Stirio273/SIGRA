namespace SIGRA.Domain.AIsupport;

public sealed class TicketKnowledgeMetadata
{
    public required string TicketId { get; init; }
    public string? Application { get; init; }
    public IReadOnlyList<string> Modules { get; init; } = [];
    public string? Category { get; init; }
    public string? Status { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
    public bool ResolutionValidated { get; init; }
}
