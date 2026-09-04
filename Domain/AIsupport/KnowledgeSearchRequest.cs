using SIGRA.Data.Enums;

namespace SIGRA.Domain.AIsupport;

public sealed class KnowledgeSearchRequest
{
    public required string Query { get; init; }

    public required int IdApplication { get; init; }

    public int ExcludeTicketId { get; init; }

    // public IReadOnlyList<string> AllowedModules { get; init; } = [];

    public int TopK { get; init; } = 5;
}

public sealed class KnowledgeSearchResult
{
    public required string SourceId { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public required string Application { get; init; }

    public string? Module { get; init; }

    public double Score { get; init; }

    public string? SourceUrl { get; init; }
    public required KnowledgeSourceType SourceType { get; init; }
}
