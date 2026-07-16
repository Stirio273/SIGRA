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

    public async Task<long> GetNextSequenceValueAsync()
    {
        var result = await _context.Database
            .SqlQuery<long>($"SELECT NEXTVAL('seq_tickets_numero_ticket') \"Value\"")
            .FirstAsync();
        return result;
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

    public async Task<Ticket?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Tickets.FirstOrDefaultAsync(t => t.IdTicket == id, ct);
    }
}
