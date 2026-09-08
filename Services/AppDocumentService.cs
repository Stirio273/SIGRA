using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Services;

public class AppDocumentService
{
    private readonly AppDbContext _dbContext;
    private readonly DocumentEmbeddingIndexer _indexer;

    public AppDocumentService(AppDbContext dbContext, DocumentEmbeddingIndexer indexer)
    {
        _dbContext = dbContext;
        _indexer = indexer;
    }

    public async Task<Result> AddApplicationDocument(AppDocument document, CancellationToken cancellationToken = default)
    {
        _dbContext.AppDocuments.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _indexer.IndexAsync(document, cancellationToken);

        return Result.Success();
    }
}