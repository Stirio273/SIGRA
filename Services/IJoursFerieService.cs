using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IJoursFerieService
{
    Task<JoursFerie> CreateAsync(CreateJoursFerieRequest req);
    Task<JoursFerie?> GetByIdAsync(int id);
    Task<IReadOnlyList<JoursFerie>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateJoursFerieRequest req);
    Task<bool> DeleteAsync(int id);
}
