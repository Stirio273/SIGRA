using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class ClassesServiceRepository : IClassesServiceRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClassesServiceRepository> _logger;

    public ClassesServiceRepository(AppDbContext context, ILogger<ClassesServiceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ClassesService>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.ClassesServices.ToListAsync(ct);
    }

    public async Task<ClassesService?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.ClassesServices.FirstOrDefaultAsync(c => c.IdCs == id, ct);
    }

    public async Task<ClassesService> CreateAsync(ClassesService classesService, CancellationToken ct = default)
    {
        _context.ClassesServices.Add(classesService);
        await _context.SaveChangesAsync(ct);
        return classesService;
    }

    public async Task UpdateAsync(ClassesService classesService, CancellationToken ct = default)
    {
        _context.ClassesServices.Update(classesService);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var classesService = await _context.ClassesServices.FirstOrDefaultAsync(c => c.IdCs == id, ct);
        if (classesService != null)
        {
            _context.ClassesServices.Remove(classesService);
            await _context.SaveChangesAsync(ct);
        }
    }
}
