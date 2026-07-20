using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface ICriticiteService
{
    Task<Criticite> CreateAsync(CreateCriticiteRequest req);
    Task<Criticite?> GetByIdAsync(int id);
    Task<IReadOnlyList<Criticite>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateCriticiteRequest req);
    Task<bool> DeleteAsync(int id);
}
