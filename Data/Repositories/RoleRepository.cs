using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<RoleRepository> _logger;

    public RoleRepository(AppDbContext context, ILogger<RoleRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Roles.ToListAsync(ct);
    }

    public async Task<Role?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.IdRole == id, ct);
    }

    public async Task<Role> CreateAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);
        return role;
    }

    public async Task UpdateAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.IdRole == id, ct);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync(ct);
        }
    }
}
