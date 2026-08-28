namespace SIGRA.Domain.AIsupport;

public sealed class AISupportResponse
{
    public required string TicketUnderstanding { get; init; }

    public IReadOnlyList<string> SuggestedSteps { get; init; }
        = Array.Empty<string>();

    public IReadOnlyList<string> PossibleCauses { get; init; }
        = Array.Empty<string>();

    public string? RecommendedEscalation { get; init; }

    public string? LimitationOrUncertainty { get; init; }

    public IReadOnlyList<AISourceReference> Sources { get; init; }
        = Array.Empty<AISourceReference>();
}

public sealed class AISourceReference
{
    public required string SourceType { get; init; }
    // Examples: InternalDocument, ResolvedTicket, OfficialDocumentation

    public required string SourceId { get; init; }

    public required string Title { get; init; }

    public string? Excerpt { get; init; }

    public string? Url { get; init; }

    public double? RelevanceScore { get; init; }
}
