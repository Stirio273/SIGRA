using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IApplicationService
{
    Task<Application> CreateAsync(CreateApplicationRequest req);
    Task<Application?> GetByIdAsync(int id);
    Task<IReadOnlyList<Application>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateApplicationRequest req);
    Task<bool> DeleteAsync(int id);
}
