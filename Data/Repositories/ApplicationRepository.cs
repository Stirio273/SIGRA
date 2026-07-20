using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<ApplicationRepository> _logger;

    public ApplicationRepository(AppDbContext context, ILogger<ApplicationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int?> GetFirstActiveApplicationIdAsync(CancellationToken ct = default)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.Actif)
            .Select(a => (int?)a.IdApplication)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Application>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Applications.ToListAsync(ct);
    }

    public async Task<Application?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Applications.FirstOrDefaultAsync(a => a.IdApplication == id, ct);
    }

    public async Task<Application> CreateAsync(Application application, CancellationToken ct = default)
    {
        _context.Applications.Add(application);
        await _context.SaveChangesAsync(ct);
        return application;
    }

    public async Task UpdateAsync(Application application, CancellationToken ct = default)
    {
        _context.Applications.Update(application);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var application = await _context.Applications.FirstOrDefaultAsync(a => a.IdApplication == id, ct);
        if (application != null)
        {
            _context.Applications.Remove(application);
            await _context.SaveChangesAsync(ct);
        }
    }
}
