using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class EntitesExterneRepository : IEntitesExterneRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EntitesExterneRepository> _logger;

    public EntitesExterneRepository(AppDbContext context, ILogger<EntitesExterneRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<EntitesExterne>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.EntitesExternes.ToListAsync(ct);
    }

    public async Task<EntitesExterne?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.EntitesExternes.FirstOrDefaultAsync(e => e.IdEntiteExterne == id, ct);
    }

    public async Task<EntitesExterne> CreateAsync(EntitesExterne entitesExterne, CancellationToken ct = default)
    {
        _context.EntitesExternes.Add(entitesExterne);
        await _context.SaveChangesAsync(ct);
        return entitesExterne;
    }

    public async Task UpdateAsync(EntitesExterne entitesExterne, CancellationToken ct = default)
    {
        _context.EntitesExternes.Update(entitesExterne);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var entitesExterne = await _context.EntitesExternes.FirstOrDefaultAsync(e => e.IdEntiteExterne == id, ct);
        if (entitesExterne != null)
        {
            _context.EntitesExternes.Remove(entitesExterne);
            await _context.SaveChangesAsync(ct);
        }
    }
}
