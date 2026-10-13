using SIGRA.Controllers;
using SIGRA.Data;
using SIGRA.Data.Models;
using SIGRA.Domain;
using Microsoft.EntityFrameworkCore;

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

    public async Task<AppDocument?> GetByIdAsync(int idAppDocument, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AppDocuments.FirstOrDefaultAsync(a => a.Id == idAppDocument);
    }

    public async Task<PagedResult<AppDocumentDto>> GetAllApplicationDocuments(
        int idApplication,
        PagedRequest pagination,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AppDocuments
            .Where(d => d.IdApplication == idApplication);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(d => d.NomFichier.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var documents = await query
            .OrderByDescending(d => d.Id)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(d => new AppDocumentDto
            (
                d.Id,
                d.Titre,
                d.NomFichier,
                d.IdApplication ?? 0,
                d.IdApplicationNavigation != null ? d.IdApplicationNavigation.Libelle : "",
                d.AppDocumentChunks.Count
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<AppDocumentDto>
        {
            Items = documents,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
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