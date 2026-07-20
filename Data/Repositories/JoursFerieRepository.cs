using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class JoursFerieRepository : IJoursFerieRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<JoursFerieRepository> _logger;

    public JoursFerieRepository(AppDbContext context, ILogger<JoursFerieRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<JoursFerie>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.JoursFeries.ToListAsync(ct);
    }

    public async Task<JoursFerie?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.JoursFeries.FirstOrDefaultAsync(j => j.IdJourFerie == id, ct);
    }

    public async Task<JoursFerie> CreateAsync(JoursFerie joursFerie, CancellationToken ct = default)
    {
        _context.JoursFeries.Add(joursFerie);
        await _context.SaveChangesAsync(ct);
        return joursFerie;
    }

    public async Task UpdateAsync(JoursFerie joursFerie, CancellationToken ct = default)
    {
        _context.JoursFeries.Update(joursFerie);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var joursFerie = await _context.JoursFeries.FirstOrDefaultAsync(j => j.IdJourFerie == id, ct);
        if (joursFerie != null)
        {
            _context.JoursFeries.Remove(joursFerie);
            await _context.SaveChangesAsync(ct);
        }
    }
}
