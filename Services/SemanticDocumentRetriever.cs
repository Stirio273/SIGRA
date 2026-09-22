using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIGRA.Data;
using SIGRA.Data.Enums;
using SIGRA.Domain.AIsupport;

namespace SIGRA.Services;

public sealed class SemanticDocumentRetriever : IDocumentationKnowledgeRetriever
{
    private readonly AppDbContext _dbContext;
    private readonly IEmbeddingService _embeddingService;

    public SemanticDocumentRetriever(AppDbContext dbContext, IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _embeddingService = embeddingService;
    }

    public async Task<IReadOnlyList<KnowledgeSearchResult>> SearchDocumentationAsync(
        KnowledgeSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var queryEmbedding = new Vector(await _embeddingService.EmbedAsync(request.Query, cancellationToken));

        var results = await _dbContext.AppDocumentChunks
            .Where(c => c.Parent.IdApplication == request.Application.IdApplication)
            .Select(c => new
            {
                c.ParentId,
                c.Content,
                Distance = c.Embedding!.CosineDistance(queryEmbedding)
            })
            .OrderBy(c => c.Distance)
            .Take(request.TopK)
            .ToListAsync(cancellationToken);

        var parentDocs = await _dbContext.AppDocuments
            .Where(d => results.Select(r => r.ParentId).Contains(d.Id))
            .Include(d => d.IdApplicationNavigation)
            .ToDictionaryAsync(d => d.Id, cancellationToken);

        return results.Select(r =>
        {
            var doc = parentDocs[r.ParentId];
            return new KnowledgeSearchResult
            {
                SourceId = $"{doc.IdApplicationNavigation?.Libelle}-DOCS-{doc.Id}",
                Title = doc.Titre,
                Content = r.Content,
                // Module = doc.Module,
                Score = 1 - r.Distance, // cosine distance -> similarity
                SourceUrl = doc.Chemin,
                Application = doc.IdApplicationNavigation?.Libelle ?? "Indeterminée"
            };
        }).ToList();
    }
}
