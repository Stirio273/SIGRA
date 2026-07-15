using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ITypesDemandeRepository
{
    Task<int?> GetTypeDemandeIdAsync(string libelle, CancellationToken ct = default);
}