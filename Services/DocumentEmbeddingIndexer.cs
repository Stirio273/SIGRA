using Pgvector;
using SIGRA.Data;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public sealed class DocumentEmbeddingIndexer
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IDocumentChunker _chunker;
    private readonly AppDbContext _dbContext;

    public DocumentEmbeddingIndexer(
        IEmbeddingService embeddingService,
        IDocumentChunker chunker,
        AppDbContext dbContext)
    {
        _embeddingService = embeddingService;
        _chunker = chunker;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Indexes (or re-indexes) a document. Safe to call any time a document 
    /// is created or its content changes — not a one-time operation.
    /// </summary>
    public async Task IndexAsync(AppDocument document, CancellationToken cancellationToken = default)
    {
        // Remove any existing chunks first, in case this is a re-index 
        // of a document whose content changed (chunk count may differ).
        var existingChunks = _dbContext.AppDocumentChunks
            .Where(c => c.ParentSourceId == document.SourceId);

        _dbContext.AppDocumentChunks.RemoveRange(existingChunks);

        var chunks = _chunker.Chunk(document.Contenu);

        for (var i = 0; i < chunks.Count; i++)
        {
            var embedding = await _embeddingService.EmbedAsync(chunks[i], cancellationToken);

            _dbContext.AppDocumentChunks.Add(new AppDocumentChunk
            {
                ParentSourceId = document.SourceId,
                ChunkIndex = i,
                Content = chunks[i],
                Embedding = new Vector(embedding)
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
