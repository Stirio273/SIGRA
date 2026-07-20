using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class JoursFerieService : IJoursFerieService
{
    private readonly IJoursFerieRepository _joursFerieRepository;

    public JoursFerieService(IJoursFerieRepository joursFerieRepository)
    {
        _joursFerieRepository = joursFerieRepository;
    }

    public async Task<JoursFerie> CreateAsync(CreateJoursFerieRequest req)
    {
        var joursFerie = new JoursFerie
        {
            Date = req.Date,
            Libelle = req.Libelle
        };

        return await _joursFerieRepository.CreateAsync(joursFerie);
    }

    public async Task<JoursFerie?> GetByIdAsync(int id)
    {
        return await _joursFerieRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<JoursFerie>> GetAllAsync()
    {
        return await _joursFerieRepository.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateJoursFerieRequest req)
    {
        var joursFerie = await _joursFerieRepository.GetByIdAsync(id);
        if (joursFerie == null)
            return false;

        joursFerie.Date = req.Date;
        joursFerie.Libelle = req.Libelle;

        await _joursFerieRepository.UpdateAsync(joursFerie);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _joursFerieRepository.DeleteAsync(id);
        return true;
    }
}
