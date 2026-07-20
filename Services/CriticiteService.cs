using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class CriticiteService : ICriticiteService
{
    private readonly ICriticiteRepository _criticiteRepository;

    public CriticiteService(ICriticiteRepository criticiteRepository)
    {
        _criticiteRepository = criticiteRepository;
    }

    public async Task<Criticite> CreateAsync(CreateCriticiteRequest req)
    {
        var criticite = new Criticite
        {
            Libelle = req.Libelle,
            Ordre = req.Ordre
        };

        return await _criticiteRepository.CreateAsync(criticite);
    }

    public async Task<Criticite?> GetByIdAsync(int id)
    {
        return await _criticiteRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Criticite>> GetAllAsync()
    {
        return await _criticiteRepository.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateCriticiteRequest req)
    {
        var criticite = await _criticiteRepository.GetByIdAsync(id);
        if (criticite == null)
            return false;

        criticite.Libelle = req.Libelle;
        criticite.Ordre = req.Ordre;

        await _criticiteRepository.UpdateAsync(criticite);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _criticiteRepository.DeleteAsync(id);
        return true;
    }
}
