using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;

    public ApplicationService(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Application> CreateAsync(CreateApplicationRequest req)
    {
        var application = new Application
        {
            Libelle = req.Libelle,
            Actif = req.Actif,
            IdCs = req.IdCs
        };

        return await _applicationRepository.CreateAsync(application);
    }

    public async Task<Application?> GetByIdAsync(int id)
    {
        return await _applicationRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<Application>> GetAllAsync()
    {
        return await _applicationRepository.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateApplicationRequest req)
    {
        var application = await _applicationRepository.GetByIdAsync(id);
        if (application == null)
            return false;

        application.Libelle = req.Libelle;
        application.Actif = req.Actif;
        application.IdCs = req.IdCs;

        await _applicationRepository.UpdateAsync(application);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _applicationRepository.DeleteAsync(id);
        return true;
    }
}
