using SIGRA.Controllers;
using SIGRA.Data.Models;
using SIGRA.Data.Repositories;

namespace SIGRA.Services;

public class ClassesServiceService : IClassesServiceService
{
    private readonly IClassesServiceRepository _classesServiceRepository;

    public ClassesServiceService(IClassesServiceRepository classesServiceRepository)
    {
        _classesServiceRepository = classesServiceRepository;
    }

    public async Task<ClassesService> CreateAsync(CreateClassesServiceRequest req)
    {
        var classesService = new ClassesService
        {
            Code = req.Code,
            Libelle = req.Libelle,
            DureeSla = req.DureeSla
        };

        return await _classesServiceRepository.CreateAsync(classesService);
    }

    public async Task<ClassesService?> GetByIdAsync(int id)
    {
        return await _classesServiceRepository.GetByIdAsync(id);
    }

    public async Task<IReadOnlyList<ClassesService>> GetAllAsync()
    {
        return await _classesServiceRepository.GetAllAsync();
    }

    public async Task<bool> UpdateAsync(int id, UpdateClassesServiceRequest req)
    {
        var classesService = await _classesServiceRepository.GetByIdAsync(id);
        if (classesService == null)
            return false;

        classesService.Code = req.Code;
        classesService.Libelle = req.Libelle;
        classesService.DureeSla = req.DureeSla;

        await _classesServiceRepository.UpdateAsync(classesService);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _classesServiceRepository.DeleteAsync(id);
        return true;
    }
}
