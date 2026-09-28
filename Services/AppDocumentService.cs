using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Domain;

namespace SIGRA.Services;

public class AppDocumentService
{
    private readonly AppDbContext _dbContext;
    private readonly DocumentEmbeddingIndexer _indexer;
    private readonly IStorageService _storageService;

    public AppDocumentService(AppDbContext dbContext, DocumentEmbeddingIndexer indexer, IStorageService storageService)
    {
        _dbContext = dbContext;
        _indexer = indexer;
        _storageService = storageService;
    }

    public async Task<Result> AddApplicationDocument(AppDocument document, CancellationToken cancellationToken = default)
    {
        _dbContext.AppDocuments.Add(document);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _indexer.IndexAsync(document, cancellationToken);

        return Result.Success();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var document = await _dbContext.AppDocuments.FindAsync(id);
        if (document == null)
            return false;

        if (!string.IsNullOrEmpty(document.Chemin))
        {
            await _storageService.DeleteAsync(document.Chemin);
        }

        var chunks = _dbContext.AppDocumentChunks.Where(c => c.ParentId == id);
        _dbContext.AppDocumentChunks.RemoveRange(chunks);

        _dbContext.AppDocuments.Remove(document);
        await _dbContext.SaveChangesAsync();

        return true;
    }
}