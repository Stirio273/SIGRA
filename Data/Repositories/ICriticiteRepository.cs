using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface ICriticiteRepository
{
    Task<int?> GetCriticiteIdAsync(string libelle, CancellationToken ct = default);
}