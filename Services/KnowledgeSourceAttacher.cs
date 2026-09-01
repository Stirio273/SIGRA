using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class KnowledgeSourceAttacher : ISourceAttacher
{
    public AISupportResponse Attach(
        AISupportResponse response,
        IReadOnlyList<KnowledgeSearchResult> knowledgeResults)
    {
        var sources = knowledgeResults
            .Select(result => new AISourceReference
            {
                SourceType = "InternalDocument",
                SourceId = result.SourceId,
                Title = result.Title,
                Excerpt = Truncate(result.Content, 200),
                RelevanceScore = result.Score
            })
            .ToList();
        response.Sources = sources;

        return response;
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength] + "...";
}
