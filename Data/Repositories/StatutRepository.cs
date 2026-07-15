using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class StatutRepository : IStatutRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<StatutRepository> _logger;

    public StatutRepository(AppDbContext context, ILogger<StatutRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> GetIdStatutByDefaultAsync(CancellationToken ct = default)
    {
        return await _context.Statuts
            .AsNoTracking()
            .Where(s => s.EstDefaut)
            .Select(s => (int?)s.IdStatut)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int?> GetStatutIdAsync(string libelle, CancellationToken ct = default)
    {
        var statut = await _context.Statuts
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Libelle == libelle, ct);
        return statut?.IdStatut;
    }
}
