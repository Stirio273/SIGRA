using Microsoft.EntityFrameworkCore;
using SIGRA.Data.Models;

namespace SIGRA.Data.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepository(AppDbContext context, ILogger<TicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Ticket> CreateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync(ct);
        return ticket;
    }

    public async Task UpdateAsync(Ticket ticket, CancellationToken ct = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<EmailSource> CreateEmailSourceAsync(EmailSource emailSource, CancellationToken ct = default)
    {
        _context.EmailSources.Add(emailSource);
        await _context.SaveChangesAsync(ct);
        return emailSource;
    }

    public async Task CreatePieceJointeAsync(PieceJointe pieceJointe, CancellationToken ct = default)
    {
        _context.PieceJointes.Add(pieceJointe);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<int?> GetFirstActiveApplicationIdAsync(CancellationToken ct = default)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(a => a.Actif)
            .Select(a => (int?)a.IdApplication)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int?> GetTypeDemandeIdAsync(string libelle, CancellationToken ct = default)
    {
        var type = await _context.TypeDemandes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Libelle == libelle, ct);
        return type?.IdTypeDemande;
    }

    public async Task<int?> GetCriticiteIdAsync(string libelle, CancellationToken ct = default)
    {
        var criticite = await _context.Criticites
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Libelle == libelle, ct);
        return criticite?.IdCriticite;
    }

    public async Task<int?> GetStatutIdAsync(string libelle, CancellationToken ct = default)
    {
        var statut = await _context.Statuts
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Libelle == libelle, ct);
        return statut?.IdStatut;
    }
}
