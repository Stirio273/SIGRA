namespace SIGRA.Domain.AIsupport;

public sealed class KnowledgeDocument
{
    public required string SourceId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public string? Module { get; init; }
}
