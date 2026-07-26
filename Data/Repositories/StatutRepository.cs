using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    public async Task<bool> IsTransitionAutoriseeAsync(int idStatutOrigine, int idStatutDestination, CancellationToken ct = default)
    {
        return await _context.Statuts
            .Where(s => s.IdStatut == idStatutOrigine)
            .SelectMany(s => s.IdStatutDestinations)
            .AnyAsync(d => d.IdStatut == idStatutDestination, ct);
    }

    public async Task<IReadOnlyList<Statut>> GetNextStatutsAsync(int idStatutOrigine, CancellationToken ct = default)
    {
        return await _context.Statuts
            .Where(s => s.IdStatut == idStatutOrigine)
            .SelectMany(s => s.IdStatutDestinations)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
