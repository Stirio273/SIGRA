using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class CommentaireRepository : ICommentaireRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<CommentaireRepository> _logger;

    public CommentaireRepository(AppDbContext context, ILogger<CommentaireRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddAsync(Commentaire commentaire, CancellationToken ct = default)
    {
        _context.Commentaires.Add(commentaire);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<Commentaire>> GetByTicketIdAsync(int ticketId, CancellationToken ct = default)
    {
        return await _context.Commentaires
            .AsNoTracking()
            .Include(c => c.IdAuteurNavigation)
            .Where(c => c.IdTicket == ticketId)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync(ct);
    }
}
