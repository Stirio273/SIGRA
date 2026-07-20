using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IJoursFerieRepository
{
    Task<IReadOnlyList<JoursFerie>> GetAllAsync(CancellationToken ct = default);
    Task<JoursFerie?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<JoursFerie> CreateAsync(JoursFerie joursFerie, CancellationToken ct = default);
    Task UpdateAsync(JoursFerie joursFerie, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
