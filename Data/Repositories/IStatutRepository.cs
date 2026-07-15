using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IStatutRepository
{
    Task<int?> GetIdStatutByDefaultAsync(CancellationToken ct = default);
    Task<int?> GetStatutIdAsync(string libelle, CancellationToken ct = default);
}
