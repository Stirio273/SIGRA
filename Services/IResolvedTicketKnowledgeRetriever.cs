using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface IResolvedTicketKnowledgeRetriever
{
    Task<IReadOnlyList<KnowledgeSearchResult>> SearchResolvedTicketsAsync(
        KnowledgeSearchRequest request, CancellationToken cancellationToken = default);
}