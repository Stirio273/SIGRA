using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IKnowledgeRetriever
{
    Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default);
}
