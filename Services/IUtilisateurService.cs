using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IUtilisateurService
{
    Task<Utilisateur> CreateAsync(CreateUtilisateurRequest req);
    Task<Utilisateur?> GetByIdAsync(int id);
    Task<Utilisateur?> GetByEmailAsync(string email);
    Task<IReadOnlyList<Utilisateur>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateUtilisateurRequest req);
    Task<bool> DeleteAsync(int id);
}
