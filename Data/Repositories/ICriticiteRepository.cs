using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ICriticiteRepository
{
    Task<int?> GetCriticiteIdAsync(string libelle, CancellationToken ct = default);
    Task<IReadOnlyList<Criticite>> GetAllAsync(CancellationToken ct = default);
    Task<Criticite?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Criticite> CreateAsync(Criticite criticite, CancellationToken ct = default);
    Task UpdateAsync(Criticite criticite, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
