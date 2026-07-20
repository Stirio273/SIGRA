using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public interface IClassesServiceRepository
{
    Task<IReadOnlyList<ClassesService>> GetAllAsync(CancellationToken ct = default);
    Task<ClassesService?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<ClassesService> CreateAsync(ClassesService classesService, CancellationToken ct = default);
    Task UpdateAsync(ClassesService classesService, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
