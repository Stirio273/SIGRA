using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class CriticiteRepository : ICriticiteRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CriticiteRepository> _logger;

    public CriticiteRepository(AppDbContext context, ILogger<CriticiteRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> GetCriticiteIdAsync(string libelle, CancellationToken ct = default)
    {
        var criticite = await _context.Criticites
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Libelle == libelle, ct);
        return criticite?.IdCriticite;
    }

    public async Task<IReadOnlyList<Criticite>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Criticites.ToListAsync(ct);
    }

    public async Task<Criticite?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Criticites.FirstOrDefaultAsync(c => c.IdCriticite == id, ct);
    }

    public async Task<Criticite> CreateAsync(Criticite criticite, CancellationToken ct = default)
    {
        _context.Criticites.Add(criticite);
        await _context.SaveChangesAsync(ct);
        return criticite;
    }

    public async Task UpdateAsync(Criticite criticite, CancellationToken ct = default)
    {
        _context.Criticites.Update(criticite);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var criticite = await _context.Criticites.FirstOrDefaultAsync(c => c.IdCriticite == id, ct);
        if (criticite != null)
        {
            _context.Criticites.Remove(criticite);
            await _context.SaveChangesAsync(ct);
        }
    }
}
