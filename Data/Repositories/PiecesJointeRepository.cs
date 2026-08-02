using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class PiecesJointeRepository : IPiecesJointeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<PiecesJointeRepository> _logger;

    public PiecesJointeRepository(AppDbContext context, ILogger<PiecesJointeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreatePieceJointeAsync(PiecesJointe pieceJointe, CancellationToken ct = default)
    {
        _context.PiecesJointes.Add(pieceJointe);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PiecesJointe?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.PiecesJointes.FirstOrDefaultAsync(p => p.IdPieceJointe == id, ct);
    }
}