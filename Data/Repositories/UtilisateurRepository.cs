using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class UtilisateurRepository : IUtilisateurRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UtilisateurRepository> _logger;

    public UtilisateurRepository(AppDbContext context, ILogger<UtilisateurRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Utilisateur>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Utilisateurs.ToListAsync(ct);
    }

    public async Task<Utilisateur?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Utilisateurs.FirstOrDefaultAsync(u => u.IdUtilisateur == id, ct);
    }

    public async Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Utilisateurs.FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<Utilisateur> CreateAsync(Utilisateur utilisateur, CancellationToken ct = default)
    {
        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync(ct);
        return utilisateur;
    }

    public async Task UpdateAsync(Utilisateur utilisateur, CancellationToken ct = default)
    {
        _context.Utilisateurs.Update(utilisateur);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var utilisateur = await _context.Utilisateurs.FirstOrDefaultAsync(u => u.IdUtilisateur == id, ct);
        if (utilisateur != null)
        {
            _context.Utilisateurs.Remove(utilisateur);
            await _context.SaveChangesAsync(ct);
        }
    }
}
