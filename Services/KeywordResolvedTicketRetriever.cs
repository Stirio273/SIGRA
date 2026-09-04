using ScottPlot;
using SIGRA.Data.Enums;
using SIGRA.Data.Repositories;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;


public sealed class KeywordResolvedTicketRetriever : IKnowledgeRetriever
{
    private readonly IResolvedTicketRepository _repository;
    private readonly IProblemRecordLookup _problemLookup;
    private readonly ITicketContentSanitizer _sanitizer;

    public KeywordResolvedTicketRetriever(
        IResolvedTicketRepository repository,
        IProblemRecordLookup problemLookup,
        ITicketContentSanitizer sanitizer)
    {
        _repository = repository;
        _problemLookup = problemLookup;
        _sanitizer = sanitizer;
    }

    public async Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var keywords = Tokenize(request.Query);

        var candidates = await _repository.GetCandidatesAsync(
            request.IdApplication,
            keywords,
            // request.AllowedModules,
            request.ExcludeTicketId,
            maxCandidates: 50,
            cancellationToken);

        var scored = new List<KnowledgeSearchResult>();

        foreach (var ticket in candidates)
        {
            var score = ComputeScore(ticket, keywords);
            if (score <= 0) continue;

            int? recurrenceCount = null;
            if (ticket.ProblemRecordId is not null)
            {
                var problem = await _problemLookup.GetAsync(
                    ticket.ProblemRecordId, cancellationToken);
                recurrenceCount = problem?.LinkedIncidentCount;
            }

            scored.Add(new KnowledgeSearchResult
            {
                SourceId = ticket.IdTicket.ToString(),
                Title = $"Resolved ticket: {ticket.Title}",
                Content = _sanitizer.Sanitize(ticket.ResolutionNotes),
                Module = ticket.Module,
                Score = score,
                SourceType = KnowledgeSourceType.ResolvedTicket,
                Application = ticket.Application
                // ResolutionType = ticket.ResolutionType,
                // RecurrenceCount = recurrenceCount
            });
        }

        return scored
            .OrderByDescending(r => r.Score)
            .Take(request.TopK)
            .ToList();
    }

    private static IReadOnlyList<string> Tokenize(string text) =>
        text.ToLowerInvariant()
            .Split([' ', '.', ',', ';', ':', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
            .Distinct()
            .ToList();

    private static int ComputeScore(ResolvedTicketSummary ticket, IReadOnlyList<string> keywords)
    {
        var content = $"{ticket.Title} {ticket.ResolutionNotes}".ToLowerInvariant();
        return keywords.Count(k => content.Contains(k));
    }
}

public interface IProblemRecordLookup
{
    Task<ProblemRecord?> GetAsync(string problemRecordId, CancellationToken cancellationToken = default);
}
