using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public interface ISourceAttacher
{
    AISupportResponse Attach(
        AISupportResponse response,
        IReadOnlyList<KnowledgeSearchResult> documentationResults,
        IReadOnlyList<KnowledgeSearchResult> ticketResults);
}
