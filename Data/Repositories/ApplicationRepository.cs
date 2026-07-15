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
}
