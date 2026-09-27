using Microsoft.Extensions.Options;
using Pgvector;
using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Domain.Options;
using System.Linq;

namespace SIGRA.Services;

public sealed class DocumentEmbeddingIndexer
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IDocumentChunker _chunker;
    private readonly AppDbContext _dbContext;
    private readonly EmbeddingServiceOptions _options;

    public DocumentEmbeddingIndexer(
        IEmbeddingService embeddingService,
        IDocumentChunker chunker,
        AppDbContext dbContext,
        IOptions<EmbeddingServiceOptions> options)
    {
        _embeddingService = embeddingService;
        _chunker = chunker;
        _dbContext = dbContext;
        _options = options.Value;
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

        foreach (var (group, batchIndex) in chunks.Chunk(_options.BatchSize).Select((g, i) => (g, i)))
        {
            var embeddings = await _embeddingService.EmbedBatchAsync(group, cancellationToken);

            if (embeddings.Length != group.Length)
                throw new InvalidOperationException(
                    $"Embedding service returned {embeddings.Length} embeddings for {group.Length} chunks.");

            var globalOffset = batchIndex * _options.BatchSize;
            for (var i = 0; i < group.Length; i++)
            {
                _dbContext.AppDocumentChunks.Add(new AppDocumentChunk
                {
                    ParentId = document.Id,
                    ChunkIndex = globalOffset + i,
                    Content = group[i],
                    Embedding = new Vector(embeddings[i])
                });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
