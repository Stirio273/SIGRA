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
               SourceType = result.SourceType.ToString(),
               SourceId = result.SourceId,
               Title = result.Title,
               Excerpt = Truncate(result.Content, 200),
               RelevanceScore = result.Score
           })
           .ToList();

        var recurring = knowledgeResults
            .Where(r => r.RecurrenceCount is > 2 && r.ResolutionType == ResolutionType.Workaround)
            .OrderByDescending(r => r.RecurrenceCount)
            .FirstOrDefault();

        response.Sources = sources;

        return response; // with
        // {
        //     Sources = sources,
        //     RecurringIssueDetected = recurring is not null,
        //     RecurrenceCount = recurring?.RecurrenceCount
        // };
    }

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength] + "...";
}
