using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IRoleService
{
    Task<IReadOnlyList<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role> CreateAsync(CreateRoleRequest req);
    Task<bool> UpdateAsync(int id, UpdateRoleRequest req);
    Task<bool> DeleteAsync(int id);
}
