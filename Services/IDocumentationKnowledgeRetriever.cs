using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IDocumentationKnowledgeRetriever
{
    // Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
    //     KnowledgeSearchRequest request,
    //     CancellationToken cancellationToken = default);

    Task<IReadOnlyList<KnowledgeSearchResult>> SearchDocumentationAsync(
    KnowledgeSearchRequest request,
    CancellationToken cancellationToken = default);
}
