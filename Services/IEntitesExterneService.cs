using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IEntitesExterneService
{
    Task<EntitesExterne> CreateAsync(CreateEntitesExterneRequest req);
    Task<EntitesExterne?> GetByIdAsync(int id);
    Task<IReadOnlyList<EntitesExterne>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateEntitesExterneRequest req);
    Task<bool> DeleteAsync(int id);
}
