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
        var existingChunks = _dbContext.AppDocumentChunks
            .Where(c => c.ParentId == document.Id);

        _dbContext.AppDocumentChunks.RemoveRange(existingChunks);

        var chunks = _chunker.Chunk(document.Contenu);

        if (chunks.Count == 0)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        foreach (var group in chunks.Chunk(batchSize))
        {
            var embeddings = await _embeddingService.EmbedBatchAsync(chunks, cancellationToken);

            if (embeddings.Length != group.Count)
                throw new InvalidOperationException(
                    $"Embedding service returned {embeddings.Length} embeddings for {group.Count} chunks.");

            for (var i = 0; i < group.Count; i++)
            {
                _dbContext.AppDocumentChunks.Add(new AppDocumentChunk
                {
                    ParentId = document.Id,
                    ChunkIndex = i,
                    Content = group[i],
                    Embedding = new Vector(embeddings[i])
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
