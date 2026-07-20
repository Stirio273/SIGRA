using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IApplicationRepository
{
    Task<int?> GetFirstActiveApplicationIdAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken ct = default);
    Task<Application?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Application> CreateAsync(Application application, CancellationToken ct = default);
    Task UpdateAsync(Application application, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
