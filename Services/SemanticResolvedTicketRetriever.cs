using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Domain.AIsupport;
using SIGRA.Date.Enums;

namespace SIGRA.Services;

public sealed class SemanticResolvedTicketRetriever : IResolvedTicketKnowledgeRetriever
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

    public async Task<IReadOnlyList<KnowledgeSearchResult>> SearchResolvedTicketsAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = new Vector(await _embeddingService.EmbedAsync(request.Query, cancellationToken));

        var query = _dbContext.Tickets
            .Where(t => t.IdStatut == (int)TicketStatus.Closed)
            .Where(t => !t.ExclureConnaissancesIa)
            .Where(t => t.Commentaires.Any(c => c.EstNoteResolution && c.EmbeddingContenu != null));

        if (request.ExcludeTicketId != 0)
            query = query.Where(t => t.IdTicket != request.ExcludeTicketId);

        var candidates = await query
            .SelectMany(
                t => t.Commentaires.Where(c => c.EstNoteResolution && c.EmbeddingContenu != null),
                (t, c) => new
                {
                    t.IdTicket,
                    t.NumeroTicket,
                    t.IdApplication,
                    t.CauseRacineIdentifie,
                    t.NombreRecurrence,
                    ApplicationName = t.IdApplicationNavigation != null ? t.IdApplicationNavigation.Libelle : null,
                    Content = c.Contenu,
                    Distance = c.EmbeddingContenu!.CosineDistance(queryEmbedding),
                    Title = t.EmailsSources.FirstOrDefault(e => e.EstEmailInitial) != null ? t.EmailsSources.FirstOrDefault(e => e.EstEmailInitial)!.Objet : null
                })
            .OrderBy(x => x.Distance)
            .Take(request.TopK)
            .ToListAsync(cancellationToken);

        var results = new List<KnowledgeSearchResult>();

        foreach (var ticket in candidates)
        {
            results.Add(new KnowledgeSearchResult
            {
                SourceId = ticket.IdTicket.ToString(),
                Title = $"Resolved ticket: {ticket.Title ?? ticket.NumeroTicket}",
                Content = _sanitizer.Sanitize(ticket.Content),
                Module = null,
                Score = 1 - ticket.Distance,
                Application = ticket.ApplicationName ?? "Indeterminée",
                RootCauseConfidence = Enum.Parse<RootCauseConfidence>(ticket.CauseRacineIdentifie),
                RecurrenceCount = ticket.NombreRecurrence
            });
        }

        return results;
    }
}
