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
}
