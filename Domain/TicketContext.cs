namespace SIGRA.Domain;

public sealed class TicketContext
{
    public required int IdTicket { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public string? Application { get; init; }

    public string? Category { get; init; }

    public string? Priority { get; init; }

    public string? Status { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    // Add only comments that the current technician is allowed to view.
    public IReadOnlyList<TicketCommentContext> Comments { get; init; }
        = Array.Empty<TicketCommentContext>();

    // Useful later for metadata-based knowledge retrieval.
    public IReadOnlyDictionary<string, string> Metadata { get; init; }
        = new Dictionary<string, string>();
}

public sealed class TicketCommentContext
{
    public required string Content { get; init; }

    // public string? AuthorRole { get; init; } // e.g. User, L1, L2, System

    public DateTimeOffset CreatedAt { get; init; }

    // public bool IsInternal { get; init; }
}
