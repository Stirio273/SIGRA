using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class UtilisateurService : IUtilisateurService
{
    private readonly IUtilisateurRepository _utilisateurRepository;

    public UtilisateurService(IUtilisateurRepository utilisateurRepository)
    {
        _utilisateurRepository = utilisateurRepository;
    }

    public async Task<Utilisateur> CreateAsync(CreateUtilisateurRequest req)
    {
        var utilisateur = new Utilisateur
        {
            IdentifiantAd = req.IdentifiantAd,
            Nom = req.Nom,
            Prenom = req.Prenom,
            Email = req.Email,
            IdRole = req.IdRole,
            Actif = req.Actif,
            DateSynchronisation = DateTime.UtcNow
        };

        return await _utilisateurRepository.CreateAsync(utilisateur);
    }

    public async Task<Utilisateur?> GetByIdAsync(int id)
    {
        return await _utilisateurRepository.GetByIdAsync(id);
    }

    public async Task<Utilisateur?> GetByEmailAsync(string email)
    {
        return await _utilisateurRepository.GetByEmailAsync(email);
    }

    public async Task<IReadOnlyList<Utilisateur>> GetAllAsync()
    {
        return await _utilisateurRepository.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateUtilisateurRequest req)
    {
        var utilisateur = await _utilisateurRepository.GetByIdAsync(id);
        if (utilisateur == null)
            return false;

        utilisateur.IdentifiantAd = req.IdentifiantAd;
        utilisateur.Nom = req.Nom;
        utilisateur.Prenom = req.Prenom;
        utilisateur.Email = req.Email;
        utilisateur.IdRole = req.IdRole;
        utilisateur.Actif = req.Actif;
        utilisateur.DateDesactivation = req.DateDesactivation;
        utilisateur.DateSynchronisation = DateTime.UtcNow;

        await _utilisateurRepository.UpdateAsync(utilisateur);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _utilisateurRepository.DeleteAsync(id);
        return true;
    }
}
