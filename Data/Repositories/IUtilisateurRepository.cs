using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IUtilisateurRepository
{
    Task<IReadOnlyList<Utilisateur>> GetAllAsync(CancellationToken ct = default);
    Task<Utilisateur?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Utilisateur?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<Utilisateur> CreateAsync(Utilisateur utilisateur, CancellationToken ct = default);
    Task UpdateAsync(Utilisateur utilisateur, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
