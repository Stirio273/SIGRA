using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class SemanticDocumentRetriever : IKnowledgeRetriever
{
    private readonly AppDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public SemanticDocumentRetriever(AppDbContext dbContext, IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<IReadOnlyList<KnowledgeSearchResult>> SearchAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = new Vector(await _embeddingService.EmbedAsync(request.Query, cancellationToken));

        var results = await _dbContext.AppDocumentChunks
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(request.TopK)
            .Select(c => new
            {
                c.ParentSourceId,
                c.Content,
                Distance = c.Embedding!.CosineDistance(queryEmbedding)
            })
            .ToListAsync(cancellationToken);

        var parentDocs = await _dbContext.AppDocuments
            .Where(d => results.Select(r => r.ParentSourceId).Contains(d.SourceId))
            .ToDictionaryAsync(d => d.SourceId, cancellationToken);

        return results.Select(r =>
        {
            var doc = parentDocs[r.ParentSourceId];
            return new KnowledgeSearchResult
            {
                SourceId = doc.SourceId,
                Title = doc.Titre,
                Content = r.Content,
                // Module = doc.Module,
                Score = 1 - r.Distance, // cosine distance -> similarity
                SourceType = KnowledgeSourceType.Documentation,
                Application = doc.IdApplicationNavigation.Libelle
            };
        }).ToList();
    }
}
