using SIGRA.Data.Enums;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class KeywordDocumentRetriever : IKnowledgeRetriever
{
    private readonly IKnowledgeDocumentStore _documentStore;

    public KeywordDocumentRetriever(IKnowledgeDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryTerms = Tokenize(request.Query);

        var scoredDocuments = _documentStore.GetAll()
            .Where(doc => request.AllowedModules.Count == 0
                || (doc.Module is not null
                    && request.AllowedModules.Contains(doc.Module)))
            .Select(doc => new
            {
                Document = doc,
                Score = ComputeScore(doc.Content, queryTerms)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(request.TopK)
            .Select(x => new KnowledgeSearchResult
            {
                SourceId = x.Document.SourceId,
                Title = x.Document.Title,
                Content = x.Document.Content,
                Module = x.Document.Module,
                Score = x.Score,
                SourceType = KnowledgeSourceType.Documentation
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<KnowledgeSearchResult>>(scoredDocuments);
    }

    private static IReadOnlyList<string> Tokenize(string text)
    {
        return text
            .ToLowerInvariant()
            .Split(
                [' ', '.', ',', ';', ':', '\n', '\r'],
                StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .ToList();
    }

    private static int ComputeScore(string content, IReadOnlyList<string> queryTerms)
    {
        var lowerContent = content.ToLowerInvariant();

        return queryTerms.Count(term => lowerContent.Contains(term));
    }
}
