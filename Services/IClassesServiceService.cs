using SIGRA.Controllers;
using SIGRA.Data.Models;

namespace SIGRA.Services;

public interface IClassesServiceService
{
    Task<ClassesService> CreateAsync(CreateClassesServiceRequest req);
    Task<ClassesService?> GetByIdAsync(int id);
    Task<IReadOnlyList<ClassesService>> GetAllAsync();
    Task<bool> UpdateAsync(int id, UpdateClassesServiceRequest req);
    Task<bool> DeleteAsync(int id);
}
