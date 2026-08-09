using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync()
    {
        return await _roleRepository.GetAllAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        return await _roleRepository.GetByIdAsync(id);
    }

    public async Task<Role> CreateAsync(CreateRoleRequest req)
    {
        var role = new Role
        {
            Libelle = req.Libelle
        };

        return await _roleRepository.CreateAsync(role);
    }

    public async Task<bool> UpdateAsync(int id, UpdateRoleRequest req)
    {
        var role = await _roleRepository.GetByIdAsync(id);
        if (role == null)
            return false;

        role.Libelle = req.Libelle;

        await _roleRepository.UpdateAsync(role);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _roleRepository.DeleteAsync(id);
        return true;
    }
}
