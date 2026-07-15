using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class TypesDemandeRepository : ITypesDemandeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TypesDemandeRepository> _logger;

    public TypesDemandeRepository(AppDbContext context, ILogger<TypesDemandeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }


    public async Task<int?> GetTypeDemandeIdAsync(string libelle, CancellationToken ct = default)
    {
        var type = await _context.TypesDemandes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Libelle == libelle, ct);
        return type?.IdTypeDemande;
    }
}
