using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class EntitesExterneService : IEntitesExterneService
{
    private readonly IEntitesExterneRepository _entitesExterneRepository;

    public EntitesExterneService(IEntitesExterneRepository entitesExterneRepository)
    {
        _entitesExterneRepository = entitesExterneRepository;
    }

    public async Task<EntitesExterne> CreateAsync(CreateEntitesExterneRequest req)
    {
        var entitesExterne = new EntitesExterne
        {
            Nom = req.Nom,
            Actif = req.Actif
        };

        return await _entitesExterneRepository.CreateAsync(entitesExterne);
    }

    public async Task<EntitesExterne?> GetByIdAsync(int id)
    {
        return await _entitesExterneRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<EntitesExterne>> GetAllAsync()
    {
        return await _entitesExterneRepository.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateEntitesExterneRequest req)
    {
        var entitesExterne = await _entitesExterneRepository.GetByIdAsync(id);
        if (entitesExterne == null)
            return false;

        entitesExterne.Nom = req.Nom;
        entitesExterne.Actif = req.Actif;

        await _entitesExterneRepository.UpdateAsync(entitesExterne);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _entitesExterneRepository.DeleteAsync(id);
        return true;
    }
}
