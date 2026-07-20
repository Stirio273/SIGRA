using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IEntitesExterneRepository
{
    Task<IReadOnlyList<EntitesExterne>> GetAllAsync(CancellationToken ct = default);
    Task<EntitesExterne?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<EntitesExterne> CreateAsync(EntitesExterne entitesExterne, CancellationToken ct = default);
    Task UpdateAsync(EntitesExterne entitesExterne, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
