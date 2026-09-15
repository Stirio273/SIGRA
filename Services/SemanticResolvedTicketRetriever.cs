using Pgvector;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class SemanticResolvedTicketRetriever : IKnowledgeRetriever
{
    private readonly AppDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;
    // private readonly IProblemRecordLookup _problemLookup;
    private readonly ITicketContentSanitizer _sanitizer;

    public SemanticResolvedTicketRetriever(
        AppDbContext dbContext,
        IEmbeddingService embeddingService,
        // IProblemRecordLookup problemLookup,
        ITicketContentSanitizer sanitizer)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
        // _problemLookup = problemLookup;
        _sanitizer = sanitizer;
    }

    public async Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = new Vector(await _embeddingService.EmbedAsync(request.Query, cancellationToken));

        var query = _dbContext.Tickets
            .Where(t => t.IdStatut == (int)TicketStatus.Closed);
        // .Where(t => !t.ExcludedFromAiKnowledgeBase)
        // .Where(t => t.ResolutionEmbedding != null);

        if (request.ExcludeTicketId != 0)
            query = query.Where(t => t.IdTicket != request.ExcludeTicketId);

        var candidates = await query
            // .OrderBy(t => t.ResolutionEmbedding!.CosineDistance(queryEmbedding))
            .Take(request.TopK)
            .Select(t => new
            {
                t.IdTicket,
                t.NumeroTicket,
                // t.ResolutionNotes,
                // t.CategoryName,
                t.IdApplication,
                // t.ResolutionType,
                // t.ProblemRecordId,
                Distance = t.Commentaires.FirstOrDefault(c => c.EstNoteResolution)?.ContenuTsv!.CosineDistance(queryEmbedding)
            })
            .ToListAsync(cancellationToken);

        var results = new List<KnowledgeSearchResult>();

        foreach (var ticket in candidates)
        {
            // int? recurrenceCount = null;
            // if (ticket.ProblemRecordId is not null)
            // {
            //     var problem = await _problemLookup.GetAsync(ticket.ProblemRecordId, cancellationToken);
            //     recurrenceCount = problem?.LinkedIncidentCount;
            // }

            results.Add(new KnowledgeSearchResult
            {
                SourceId = ticket.Id,
                Title = $"Resolved ticket: {ticket.Title}",
                Content = _sanitizer.Sanitize(ticket.ResolutionNotes!),
                Module = ticket.CategoryName,
                Score = 1 - ticket.Distance,
                SourceType = KnowledgeSourceType.ResolvedTicket,
                Application = ticket.ApplicationArea,
                // ResolutionType = ticket.ResolutionType,
                // RecurrenceCount = recurrenceCount
            });
        }

        return results;
    }
}
