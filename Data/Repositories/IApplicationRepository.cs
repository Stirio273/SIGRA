using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IApplicationRepository
{
    Task<int?> GetFirstActiveApplicationIdAsync(CancellationToken ct = default);
}