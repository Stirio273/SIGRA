using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class CompositeKnowledgeRetriever : IKnowledgeRetriever
{
    private readonly IReadOnlyList<IKnowledgeRetriever> _retrievers;

    public CompositeKnowledgeRetriever(IEnumerable<IKnowledgeRetriever> retrievers)
    {
        _retrievers = retrievers.ToList();
    }

    public async Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var allResults = new List<KnowledgeSearchResult>();

        foreach (var retriever in _retrievers)
        {
            var results = await retriever.SearchAsync(request, cancellationToken);
            allResults.AddRange(results);
        }

        return allResults
            .OrderByDescending(r => r.Score)
            .Take(request.TopK)
            .ToList();
    }
}
